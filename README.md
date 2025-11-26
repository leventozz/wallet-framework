# Wallet Framework

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)
![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-3.13-FF6600?logo=rabbitmq)
![Redis](https://img.shields.io/badge/Redis-7-DC382D?logo=redis)
![MassTransit](https://img.shields.io/badge/MassTransit-8.x-00A8E6?logo=masstransit)
![EF Core](https://img.shields.io/badge/EF%20Core-10-512BD4?logo=dotnet)
![Dapper](https://img.shields.io/badge/Dapper-2.1-1C82AD?logo=dapper)
![Prometheus](https://img.shields.io/badge/Prometheus-Latest-E6522C?logo=prometheus)
![Jaeger](https://img.shields.io/badge/Jaeger-Latest-1296B8?logo=jaeger)
![Grafana](https://img.shields.io/badge/Grafana-Latest-F46800?logo=grafana)
![MediatR](https://img.shields.io/badge/MediatR-12.x-512BD4?logo=dotnet)
![Keycloak](https://img.shields.io/badge/Keycloak-Latest-469DE2?logo=keycloak)
![HashiCorp Vault](https://img.shields.io/badge/HashiCorp%20Vault-Latest-000000?logo=vault)
![YARP](https://img.shields.io/badge/YARP-Reverse%20Proxy-512BD4?logo=dotnet)
![OpenTelemetry](https://img.shields.io/badge/OpenTelemetry-Latest-000000?logo=opentelemetry)
![License](https://img.shields.io/badge/License-MIT-green)

## Executive Summary

**Wallet Framework** is a distributed P2P (Peer-to-Peer) money transfer system built with Domain-Driven Design (DDD), CQRS, and Event-Driven Architecture principles, designed for high scalability and resilience.

## Key Features

| Feature | Implementation |
|---------|---------------|
| **Microservice Architecture** | 4 core services: Transaction, Wallet, Customer, Fraud |
| **Distributed Transaction Management** | Saga Pattern (Orchestration) via MassTransit State Machine |
| **Data Consistency** | Outbox/Inbox Pattern with EF Core |
| **Centralized Security** | YARP Gateway + Keycloak (OIDC/OAuth 2.0) |
| **Full Observability** | Jaeger (Traces), Prometheus (Metrics), Grafana (Dashboards) |
| **Secrets Management** | HashiCorp Vault |

## Tech Stack

| Category | Technology |
|----------|------------|
| **Runtime** | .NET 10 |
| **ORM** | Entity Framework Core 10 |
| **Messaging** | MassTransit v8.x + RabbitMQ 3.13 |
| **CQRS** | MediatR v12.x |
| **Database** | PostgreSQL 16 |
| **Cache** | Redis 7 |
| **Identity** | Keycloak |
| **Gateway** | YARP |
| **Secrets** | HashiCorp Vault |
| **Observability** | OpenTelemetry, Jaeger, Prometheus, Grafana |

## Quick Start

### Prerequisites

- Docker Desktop (or Docker Engine + Docker Compose)
- .NET 10 SDK (for local development)

### Run the Application

Start all services with a single command:

```bash
docker-compose up -d
```

This will start:
- All microservices (Customer, Wallet, Transaction, Fraud)
- API Gateway
- Infrastructure services (PostgreSQL, RabbitMQ, Redis, Keycloak, Vault)
- Observability stack (Jaeger, Prometheus, Grafana)

### Service Endpoints

Once the services are running, you can access:

| Service | Port | URL |
|---------|------|-----|
| **API Gateway** | 5000 | http://localhost:5000 |
| **Keycloak** | 8080 | http://localhost:8080 |
| **RabbitMQ Management UI** | 15672 | http://localhost:15672 |
| **Jaeger UI** | 16686 | http://localhost:16686 |
| **Grafana** | 3000 | http://localhost:3000 |
| **Prometheus** | 9090 | http://localhost:9090 |
| **PgAdmin** | 5050 | http://localhost:5050 |
| **Vault UI** | 8200 | http://localhost:8200 |

**Default Credentials:**
- RabbitMQ: `user` / `password`
- Grafana: `admin` / `admin`
- PgAdmin: `admin@example.com` / `admin`
- Keycloak: `admin` / `admin`
- Vault: Root token `my-root-token` (dev mode)

## Project Structure

```
WalletFramework/
├── src/
│   ├── Infrastructure/
│   │   └── WF.ApiGateway/          # YARP API Gateway
│   └── Services/
│       ├── CustomerService/        # Customer management
│       ├── FraudService/           # Fraud detection
│       ├── TransactionService/     # Transaction orchestration
│       └── WalletService/          # Wallet operations
├── shared/
│   ├── WF.Shared.Contracts/        # Shared contracts and DTOs
│   └── WF.Shared.Observability/    # OpenTelemetry configuration
├── tests/                          # Unit and integration tests
├── docs/                           # Documentation
└── docker-compose.yml              # Infrastructure orchestration
```

## Documentation

For detailed information, please refer to the following documentation:

- **[Architecture](docs/architecture.md)** - Architecture decisions, diagrams, DDD, CQRS
- **[Getting Started](docs/getting-started.md)** - Setup, Docker, Seed Data, First Run
- **[Services](docs/services.md)** - Microservices overview and responsibilities
- **[Patterns and Practices](docs/patterns-and-practices.md)** - Design patterns: Result, Value Objects, Smart Enum, Saga
- **[Security](docs/security.md)** - Keycloak, Vault, Authentication flows
- **[Observability](docs/observability.md)** - Jaeger, Prometheus, Grafana dashboards

## Contributing

This project follows Clean Architecture principles with strict separation of concerns. Please ensure all code adheres to:

- SOLID principles (especially Single Responsibility and Dependency Inversion)
- C# 12+ language features (global using, file-scoped namespaces, primary constructors)
- Async/await for I/O-bound operations
- Structured logging with message templates (no string interpolation in logs)
- Domain-Driven Design patterns

## License

MIT License

