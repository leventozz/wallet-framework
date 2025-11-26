# Services

> **Status:** Documentation in progress

This document provides a comprehensive overview of all microservices in the Wallet Framework ecosystem, their responsibilities, bounded contexts, and technical capabilities.

---

## Microservices Overview

Wallet Framework follows a microservices architecture pattern with clear service boundaries, event-driven communication, and database-per-service isolation. The system is designed for high scalability, resilience, and maintainability.

Each service represents a distinct bounded context in the Domain-Driven Design (DDD) approach, ensuring clear separation of concerns and independent evolution. Services communicate asynchronously via RabbitMQ using MassTransit, while synchronous operations use HTTP through the API Gateway.

---

## Service Matrix

| Service Name | Role | Database | Main Patterns |
|--------------|------|----------|---------------|
| **ApiGateway** | Entry Point | Stateless | YARP, Rate Limiting, Auth Proxy |
| **IdentityService** | Identity Provider | PostgreSQL (Keycloak) | OIDC, OAuth2, RBAC |
| **CustomerService** | Profile Management | WF_CustomerDb (PostgreSQL) | CQRS, Outbox, Read Model Replication |
| **WalletService** | Balance Management | WF_WalletDb (PostgreSQL) | DDD, Optimistic Concurrency, Inbox Pattern |
| **TransactionService** | Orchestration | WF_TransactionDb (PostgreSQL) | Saga State Machine, Outbox |
| **FraudService** | Risk Control | WF_FraudDb (PostgreSQL) | Strategy Pattern, Rules Engine, Dapper |

---

## Service Details

### 1. API Gateway (YARP)

The single entry point to the system for external access.

**Responsibilities:**
- **Routing**: Request routing to all microservices (Customer, Wallet, Transaction, Fraud, Identity)
- **Rate Limiting**: Redis-backed IP-based rate limiting (60 req/min general, 10 req/min for auth endpoints)
- **SSL Termination**: HTTPS traffic management
- **Auth Proxy**: JWT token pass-through (forwarding tokens from Keycloak to backend services)
- **CORS Configuration**: CORS policies in development environment
- **Request Transformation**: Path pattern transformations (e.g., `/api/v1/auth/register` → `/api/v1/customers`)

**Security:**
- DDoS protection with IP-based rate limiting
- Centralized authentication with token pass-through
- Endpoint-specific rate limit rules (stricter for auth endpoints)

**Configuration:**
- YARP reverse proxy configuration: [appsettings.Development.json](src/Infrastructure/WF.ApiGateway/appsettings.Development.json)
- Rate limiting rules: 60 req/min general, 10 req/min for auth endpoints
- Observability: Tracing and metrics export with OpenTelemetry

---

### 2. Customer Service

Service that manages user identity and profile information.

**Bounded Context:** Customer Profile, KYC Status, Contact Information

**Aggregate Root:**
- **`Customer`**: [Customer.cs](src/Services/CustomerService/WF.CustomerService.Domain/Entities/Customer.cs)
  - Customer lifecycle management (create, update, soft delete)
  - KYC status tracking
  - Profile information management

**Value Objects:**
- **`Email`**: Email format and length validation
- **`PersonName`**: First name and last name information
- **`PhoneNumber`**: Phone number format and validation

**Critical Features:**
- **Registration Flow**: Keycloak IdentityId mapping - Customer entity is created with IdentityId from Keycloak during new customer registration
- **KYC Status Management**: Tracks Unverified, Pending, Verified statuses
- **Soft Delete**: Customer deletion is performed as soft delete (IsDeleted flag)
- **Read-Side Replication**: WalletReadModel is created and updated from events received from WalletService (CQRS Read Model)

**Data Model:**
- `Customer` entity: Id, IdentityId, CustomerNumber, Name, Email, PhoneNumber, KycStatus, CreatedAtUtc, UpdatedAtUtc, IsDeleted, IsActive
- `WalletReadModel`: Read model created from events received from WalletService (for Customer-Wallet relationship)

**Events:**
- **Published:** `CustomerCreatedEvent` - Notification to WalletService when a new customer is created
- **Subscribed:** 
  - `WalletCreatedEvent` - Create WalletReadModel
  - `WalletBalanceUpdatedEvent` - Update WalletReadModel
  - `WalletStateChangedEvent` - Update WalletReadModel state

---

### 3. Wallet Service

Core service that holds and manages financial assets (balances).

**Bounded Context:** Wallets, Balances, Currencies

**Aggregate Root:**
- **`Wallet`**: [Wallet.cs](src/Services/WalletService/WF.WalletService.Domain/Entities/Wallet.cs)
  - Balance management (deposit, withdraw)
  - Wallet state management (active, frozen, closed)
  - Transaction history tracking

**Value Objects:**
- **`Money`**: [Money.cs](src/Services/WalletService/WF.WalletService.Domain/ValueObjects/Money.cs)
  - Encapsulation of Amount and Currency
  - Type-safe arithmetic operators (+, -, <, >, <=, >=)
  - Currency mixing protection (operations with different currencies are not allowed)
  - Decimal precision is preserved
- **`Iban`**: International Bank Account Number validation and formatting

**Critical Features:**
- **Money Value Object Usage**: All monetary operations are performed through Money value object, ensuring type safety
- **Decimal Precision**: Decimal precision is preserved through EF Core configuration (currency precision)
- **Concurrency Management**: Data consistency in concurrent operations is ensured with optimistic concurrency control
- **Idempotency with Inbox Pattern**: MassTransit Inbox pattern prevents processing the same command multiple times
- **Wallet State Management**: Wallet lifecycle is managed with Active, Frozen, Closed states
- **Automatic Wallet Creation with CustomerCreated Event**: Wallet is automatically created when a new customer is created

**Events:**
- **Published:**
  - `WalletCreatedEvent` - When a new wallet is created
  - `WalletDebitedEvent` - When money is withdrawn from wallet
  - `WalletCreditedEvent` - When money is deposited to wallet
  - `WalletDebitFailedEvent` - When withdrawal operation fails
  - `WalletCreditFailedEvent` - When deposit operation fails
  - `WalletBalanceUpdatedEvent` - When balance is updated
  - `WalletStateChangedEvent` - When wallet state changes (frozen, closed, etc.)
  - `SenderRefundedEvent` - When refund operation is completed
- **Subscribed:**
  - `CustomerCreatedEvent` - Automatic wallet creation for new customer
- **Subscribed Commands:**
  - `DebitSenderWalletCommandContract` - Withdraw from sender wallet
  - `CreditWalletCommandContract` - Deposit to receiver wallet
  - `RefundSenderWalletCommandContract` - Refund to sender wallet

---

### 4. Transaction Service (Saga Orchestrator)

Service that manages and coordinates the distributed money transfer process.

**Bounded Context:** Transfer Requests, Transaction History, Saga State

**Aggregate Root:**
- **`Transaction`**: [Transaction.cs](src/Services/TransactionService/WF.TransactionService.Domain/Entities/Transaction.cs)
  - Saga State Machine Instance (MassTransit SagaStateMachineInstance)
  - P2P transfer orchestration
  - Transaction state management
  - Failure tracking and compensation

**Critical Features:**
- **MassTransit State Machine Usage**: [TransferSagaStateMachine.cs](src/Services/TransactionService/WF.TransactionService.Infrastructure/Features/Sagas/TransferSagaStateMachine.cs)
- **Saga States**: 
  - `Initial` → `Pending` → `SenderDebitPending` → `ReceiverCreditPending` → `Completed` / `Failed`
- **"Happy Path" Scenario**: Fraud Check → Wallet Debit → Wallet Credit → Completed
- **Compensation Scenario**: When WalletCreditFailed, refund operation to sender is automatically initiated

**Flow:**
1. **TransferRequestStarted**: Transaction is initiated, saga instance is created
2. **Fraud Check**: `CheckFraudCommandContract` is sent to FraudService
3. **FraudCheckApproved**: Fraud check successful, `DebitSenderWalletCommandContract` is sent
4. **WalletDebited**: Money withdrawn from sender wallet, `CreditWalletCommandContract` is sent
5. **WalletCredited**: Money deposited to receiver wallet, transaction completed → `Completed`
6. **Error Scenarios**:
   - FraudCheckDeclined → `Failed` (no compensation needed as no money movement occurred)
   - WalletDebitFailed → `Failed` (no compensation needed as no money movement occurred)
   - WalletCreditFailed → `RefundSenderWalletCommandContract` is sent → `Failed` (compensation completed)

**Service-to-Service Communication:**
- **HTTP Clients**: CustomerServiceApiClient, WalletServiceApiClient (for synchronous operations)
- **Keycloak Service Token Management**: Token cache mechanism for service-to-service authentication

**Events:**
- **Published Commands:**
  - `CheckFraudCommandContract` - For fraud check
  - `DebitSenderWalletCommandContract` - Withdraw from sender wallet
  - `CreditWalletCommandContract` - Deposit to receiver wallet
  - `RefundSenderWalletCommandContract` - Refund to sender wallet
- **Subscribed:**
  - `FraudCheckApprovedEvent` - Fraud check successful
  - `FraudCheckDeclinedEvent` - Fraud check failed
  - `WalletDebitedEvent` - Withdrawal successful
  - `WalletDebitFailedEvent` - Withdrawal failed
  - `WalletCreditedEvent` - Deposit successful
  - `WalletCreditFailedEvent` - Deposit failed

---

### 5. Fraud Service

Decision mechanism that audits transactions according to specific rules and approves or rejects them.

**Bounded Context:** Blacklists, Risk Rules, Transaction Limits

**Aggregate Roots:**
- **`AccountAgeRule`**: [AccountAgeRule.cs](src/Services/FraudService/WF.FraudService.Domain/Entities/AccountAgeRule.cs) - Account age rules
- **`BlockedIpRule`**: [BlockedIpRule.cs](src/Services/FraudService/WF.FraudService.Domain/Entities/BlockedIpRule.cs) - Blocked IP addresses
- **`KycLevelRule`**: [KycLevelRule.cs](src/Services/FraudService/WF.FraudService.Domain/Entities/KycLevelRule.cs) - KYC level requirements
- **`RiskyHourRule`**: [RiskyHourRule.cs](src/Services/FraudService/WF.FraudService.Domain/Entities/RiskyHourRule.cs) - Risky time ranges

**Value Objects:**
- **`IpAddress`**: IP address validation and formatting
- **`Money`**: Shared money value object (for fraud rules)
- **`TimeRange`**: Risky time range definition (start hour, end hour)

**Critical Features:**
- **Rule Management with Strategy Pattern**: Each rule is implemented as an independent strategy through the `IFraudEvaluationRule` interface
- **Priority-Based Rule Evaluation**: Rules are evaluated sequentially by priority (BlockedIp: 1, RiskyHour: 2, etc.)
- **High-Performance Rule Reading with Dapper**: Dapper is used for read operations (instead of EF Core)
- **"Fail-Fast" Mechanism**: Evaluation stops at the first failed rule, not all rules are checked

**Rules:**
- **BlockedIpRule**: Blocked IP check - Transactions from specific IP addresses are rejected
- **RiskyHourRule**: Risky time range check - Transactions made during specific time ranges (e.g., 02:00-05:00) are rejected
- **KycLevelRule**: KYC level and maximum transaction amount check - Transaction limits are applied based on KYC level
- **AccountAgeRule**: Minimum account age and maximum transaction amount check - Lower limits for new accounts

**Events:**
- **Published:**
  - `FraudCheckApprovedEvent` - Fraud check successful
  - `FraudCheckDeclinedEvent` - Fraud check failed (with reason)
- **Subscribed Commands:**
  - `CheckFraudCommandContract` - Fraud check request

---

## Inter-Service Communication

### Synchronous Communication
- **Client → API Gateway → Services**: All external requests pass through YARP API Gateway
- **Service-to-Service (HTTP)**: TransactionService makes synchronous HTTP calls to CustomerService and WalletService
- **Authentication**: All service endpoints (except health checks) are protected with Keycloak JWT Bearer tokens

### Asynchronous Communication
- **Event-Driven**: Services communicate asynchronously via RabbitMQ using MassTransit
- **Integration Events**: Domain events are published as integration events, ensuring loose coupling
- **Commands**: Services send commands to each other (e.g., TransactionService → FraudService)
- **Outbox Pattern**: Event publishing and database transaction are performed in the same atomic operation

---

## API Contracts

All services follow RESTful API standards and use versioning (`/api/v1/...`).

### Shared Contracts
- **Location**: [WF.Shared.Contracts](shared/WF.Shared.Contracts/)
- **Integration Events**: [IntegrationEvents](shared/WF.Shared.Contracts/IntegrationEvents/)
- **Command Contracts**: [Commands](shared/WF.Shared.Contracts/Commands/)
- **DTOs**: [Dtos](shared/WF.Shared.Contracts/Dtos/)

### Service Endpoints
Each service exposes its own API endpoints:
- CustomerService: `/api/v1/customers/*`
- WalletService: `/api/v1/wallets/*`
- TransactionService: `/api/v1/transactions/*`
- FraudService: `/api/v1/frauds/*` (public), `/api/v1/admin/fraud/*` (admin)

---

## Additional Resources

- [Architecture Documentation](architecture.md) - System architecture and design decisions
- [Getting Started Guide](getting-started.md) - Setup and first run
- [Patterns and Practices](patterns-and-practices.md) - Implementation patterns
- [Security Documentation](security.md) - Authentication and authorization
