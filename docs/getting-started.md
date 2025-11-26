# Getting Started

This guide will walk you through setting up the Wallet Framework project on your local machine. Follow these steps to get the entire system running from scratch.

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Installation Steps](#installation-steps)
3. [Initial Data Seeding](#initial-data-seeding)
4. [Keycloak Configuration](#keycloak-configuration-critical-step)
5. [Creating Test Users](#creating-test-users)
6. [Testing the Setup](#testing-the-setup)
7. [Troubleshooting](#troubleshooting)

---

## Prerequisites

Before you begin, ensure you have the following tools installed on your system:

| Tool | Minimum Version | Purpose | Download |
|------|----------------|---------|----------|
| **Docker Desktop** | Latest | Container orchestration for all services | [Docker Desktop](https://www.docker.com/products/docker-desktop) |
| **.NET 8 SDK** | 8.0+ | Required for local development (optional if only running via Docker) | [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) |
| **Git** | Latest | Repository cloning | [Git](https://git-scm.com/downloads) |
| **IDE** | - | Code editing (VS Code, Visual Studio, or Rider) | - |

### Verify Prerequisites

```bash
# Check Docker
docker --version
docker-compose --version

# Check .NET SDK (if developing locally)
dotnet --version

# Check Git
git --version
```

---

## Installation Steps

### Step 1: Clone the Repository

```bash
git clone <repository-url>
cd WalletFramework
```

Replace `<repository-url>` with your actual repository URL.

### Step 2: Start Infrastructure Services

The project uses Docker Compose to orchestrate all services. Start all containers with a single command:

```bash
docker-compose up -d
```

This command will:
- Pull required Docker images (if not already present)
- Create Docker networks and volumes
- Start all services in detached mode (`-d` flag)

#### Container Startup Order

Docker Compose automatically handles dependencies. Services start in this order:

1. **Infrastructure Services** (PostgreSQL, RabbitMQ, Redis, Vault)
2. **Keycloak** (depends on PostgreSQL)
3. **Observability Stack** (Prometheus, Grafana, Jaeger)
4. **Microservices** (CustomerService, WalletService, TransactionService, FraudService)
5. **API Gateway** (depends on all services)

#### Monitor Startup Progress

Watch container logs to verify services are starting correctly:

```bash
# View all logs
docker-compose logs -f

# View logs for a specific service
docker-compose logs -f customerservice
docker-compose logs -f keycloak
```

### Step 3: Verify Services are Running

Check that all containers are running:

```bash
docker-compose ps
```

You should see all services with status `Up`. Wait 30-60 seconds for all services to fully initialize, especially Keycloak and the microservices.

#### Expected Container List

| Container | Status | Ports |
|-----------|--------|-------|
| `postgres_db2` | Up | 5432:5432 |
| `pgadmin_gui` | Up | 5050:80 |
| `rabbitmq_broker` | Up | 5672:5672, 15672:15672 |
| `redis_cache` | Up | 6379:6379 |
| `keycloak_idm` | Up | 8080:8080 |
| `vault_secrets` | Up | 8200:8200 |
| `prometheus_metrics` | Up | 9090:9090 |
| `grafana_dashboard` | Up | 3000:3000 |
| `jaeger_tracing` | Up | 16686:16686, 4317:4317 |
| `wf_apigateway` | Up | 5000:8080 |
| `wf_customerservice` | Up | 7001:8080 |
| `wf_walletservice` | Up | 7004:8080 |
| `wf_transactionservice` | Up | 7003:8080 |
| `wf_fraudservice` | Up | 7002:8080 |
| `mailhog` | Up | 1025:1025, 8025:8025 |

---

## Initial Data Seeding

### Database Initialization

When PostgreSQL starts for the first time, it automatically executes [init-db.sql](init-db.sql) to create all required databases:

- `WF_KeycloakDb` - Keycloak identity and access management database
- `WF_CustomerDb` - Customer service database
- `WF_WalletDb` - Wallet service database
- `WF_TransactionDb` - Transaction service database
- `WF_FraudDb` - Fraud detection service database

**Note:** The script runs automatically on first container startup. If you need to reset databases, remove the PostgreSQL volume:

```bash
docker-compose down -v  # Removes volumes
docker-compose up -d    # Recreates everything
```

### Keycloak Realm Import

Keycloak automatically imports the realm configuration from [wallet-realm.json](wallet-realm.json) on startup. This includes:

- **Realm Name:** `wallet-realm`
- **Client:** `wallet-client` (OAuth 2.0 / OIDC client)
- **Service Account:** Pre-configured service account for inter-service authentication
- **Roles and Scopes:** Default realm roles and client scopes

**Important:** The realm import happens automatically when Keycloak starts with the `--import-realm` flag. You should see a log message confirming the import:

```
Added realm 'wallet-realm' from file /opt/keycloak/data/import/realm.json
```

---

## Keycloak Configuration (Critical Step)

### Regenerate Client Secret

**⚠️ IMPORTANT:** The client secret in the realm JSON file is masked (`**********`). You **must** regenerate it in Keycloak UI and update service configurations.

#### Step-by-Step: Regenerate Client Secret

1. **Access Keycloak Admin Console**
   - Navigate to: http://localhost:8080
   - Click "Administration Console"
   - Login with:
     - **Username:** `admin`
     - **Password:** `admin`

2. **Select the Wallet Realm**
   - In the top-left dropdown, select `wallet-realm` (not `master`)

3. **Navigate to Clients**
   - In the left sidebar, click **Clients**
   - Find and click on `wallet-client`

4. **Regenerate Client Secret**
   - Click on the **Credentials** tab
   - Click the **Regenerate** button next to "Client Secret"
   - **Copy the new secret immediately** (you won't be able to see it again)

5. **Save the Secret**
   - Store the secret securely (you'll need it in the next step)

### Update Service Configuration

After regenerating the client secret, you must update it in service configurations. Choose one of the following methods:

#### Option A: Environment Variables (Docker Compose) - Quick Setup

Update the `Keycloak__ClientSecret` environment variable in [docker-compose.yml](docker-compose.yml) for all services:

```yaml
# Example for CustomerService
customerservice:
  environment:
    Keycloak__ClientSecret: <YOUR_NEW_CLIENT_SECRET>  # Replace this
```

**Services to update:**
- `customerservice`
- `walletservice`
- `transactionservice`
- `fraudservice`
- `apigateway`

After updating, restart the affected services:

```bash
docker-compose restart customerservice walletservice transactionservice fraudservice apigateway
```

#### Option B: HashiCorp Vault (Recommended for Production)

Store the client secret in Vault for centralized secret management:

1. **Access Vault UI**
   - Navigate to: http://localhost:8200
   - Login with token: `my-root-token` (dev mode)

2. **Store the Secret**
   - Navigate to: **Secrets** → **secret** → **wallet** → **shared**
   - Create/Update key: `Keycloak__ClientSecret`
   - Value: `<YOUR_NEW_CLIENT_SECRET>`

3. **Verify Service Configuration**
   - Services automatically reload secrets from Vault every 600 seconds (10 minutes)
   - Check service logs to confirm Vault connection:
     ```bash
     docker-compose logs customerservice | grep -i vault
     ```

#### Option C: Local Configuration Files (For Local Development)

If running services locally (not via Docker), update `appsettings.Development.json`:

| Service | Configuration File |
|---------|-------------------|
| CustomerService | `src/Services/CustomerService/WF.CustomerService.Api/appsettings.Development.json` |
| WalletService | `src/Services/WalletService/WF.WalletService.Api/appsettings.Development.json` |
| TransactionService | `src/Services/TransactionService/WF.TransactionService.Api/appsettings.Development.json` |
| FraudService | `src/Services/FraudService/WF.FraudService.Api/appsettings.Development.json` |
| ApiGateway | `src/Infrastructure/WF.ApiGateway/appsettings.json` |

Update the `Keycloak` section:

```json
{
  "Keycloak": {
    "BaseUrl": "http://localhost:8080",
    "Realm": "wallet-realm",
    "ClientId": "wallet-client",
    "ClientSecret": "<YOUR_NEW_CLIENT_SECRET>"
  }
}
```

---

## Creating Test Users

To test the system, you need to create users in Keycloak. Users can then register via the CustomerService API, but for initial testing, you can create them directly in Keycloak.

### Create User via Keycloak Admin Console

1. **Navigate to Users**
   - In Keycloak Admin Console (wallet-realm)
   - Click **Users** in the left sidebar
   - Click **Add user** button

2. **Fill User Details**
   - **Username:** (e.g., `testuser1`)
   - **Email:** (e.g., `testuser1@example.com`)
   - **First name:** (optional)
   - **Last name:** (optional)
   - Toggle **Email verified** to ON (for testing)
   - Click **Create**

3. **Set Password**
   - Click on the **Credentials** tab
   - Click **Set password**
   - Enter password (e.g., `Password123!`)
   - Toggle **Temporary** to OFF (so password doesn't expire)
   - Click **Save**
   - Confirm by clicking **Set password** in the dialog

4. **Assign Roles (Optional)**
   - Click on the **Role mapping** tab
   - Assign realm roles if needed (usually not required for basic testing)

### Create User via CustomerService API

Alternatively, users can register via the CustomerService API. See the [Services Documentation](services.md) for API details.

---

## Testing the Setup

### Verify Infrastructure Components

#### PostgreSQL

```bash
# Check PostgreSQL is ready
docker exec postgres_db2 pg_isready -U postgres

# Or access via PgAdmin
# Navigate to: http://localhost:5050
# Login: admin@example.com / admin
# Add server: postgres:5432, user: postgres, password: myStrongPassword123!
```

#### RabbitMQ

- **Management UI:** http://localhost:15672
- **Username:** `user`
- **Password:** `password`
- Verify queues and exchanges are created by services

#### Redis

```bash
# Test Redis connection
docker exec redis_cache redis-cli ping
# Should return: PONG
```

#### Keycloak

- **Admin Console:** http://localhost:8080
- **Realm:** `wallet-realm`
- Verify realm is imported and `wallet-client` exists

#### HashiCorp Vault

- **UI:** http://localhost:8200
- **Token:** `my-root-token` (dev mode)
- Verify secrets can be stored and retrieved

### Test API Access via Swagger

Each service exposes a Swagger UI for API testing:

| Service | Swagger URL |
|---------|-------------|
| **API Gateway** | http://localhost:5000/swagger |
| **CustomerService** | http://localhost:7001/swagger |
| **WalletService** | http://localhost:7004/swagger |
| **TransactionService** | http://localhost:7003/swagger |
| **FraudService** | http://localhost:7002/swagger |

**Note:** Most endpoints require authentication. Use the "Authorize" button in Swagger UI with a Bearer token.

### Get Access Token

To authenticate API requests, you need to obtain a JWT token from Keycloak:

#### Using curl

```bash
curl -X POST "http://localhost:8080/realms/wallet-realm/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password" \
  -d "client_id=wallet-client" \
  -d "client_secret=<YOUR_CLIENT_SECRET>" \
  -d "username=<YOUR_USERNAME>" \
  -d "password=<YOUR_PASSWORD>"
```

**Response:**
```json
{
  "access_token": "eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICJ...",
  "expires_in": 300,
  "refresh_expires_in": 1800,
  "token_type": "Bearer",
  ...
}
```

#### Using Swagger UI

1. Open any service's Swagger UI (e.g., http://localhost:7001/swagger)
2. Click the **Authorize** button (lock icon)
3. In the "Value" field, paste your `access_token` from the curl response
4. Click **Authorize**, then **Close**
5. Now you can test authenticated endpoints

### Test Health Checks

All services expose health check endpoints:

```bash
# API Gateway
curl http://localhost:5000/health

# CustomerService
curl http://localhost:7001/health

# WalletService
curl http://localhost:7004/health

# TransactionService
curl http://localhost:7003/health

# FraudService
curl http://localhost:7002/health
```

Expected response: `Healthy` or JSON health status.

### Verify Observability Stack

- **Jaeger UI:** http://localhost:16686 - View distributed traces
- **Prometheus:** http://localhost:9090 - View metrics
- **Grafana:** http://localhost:3000 - View dashboards (login: `admin` / `admin`)

---

## Troubleshooting

### Container Startup Failures

**Problem:** Containers fail to start or exit immediately.

**Solutions:**
1. Check logs: `docker-compose logs <service-name>`
2. Verify ports are not in use: `netstat -an | grep <port>` (Windows) or `lsof -i :<port>` (Mac/Linux)
3. Ensure Docker has enough resources (CPU/Memory)
4. Try removing volumes and restarting: `docker-compose down -v && docker-compose up -d`

### Database Connection Errors

**Problem:** Services cannot connect to PostgreSQL.

**Solutions:**
1. Verify PostgreSQL is running: `docker-compose ps postgres`
2. Check PostgreSQL logs: `docker-compose logs postgres`
3. Verify database exists: Connect via PgAdmin and check for `WF_*` databases
4. Check connection string in service configuration matches docker-compose.yml

### Authentication Errors

**Problem:** "401 Unauthorized" or "Invalid token" errors.

**Solutions:**
1. **Verify client secret is updated** in all service configurations
2. Check token expiration (default: 5 minutes)
3. Verify Keycloak is accessible: http://localhost:8080
4. Check Keycloak logs: `docker-compose logs keycloak`
5. Ensure you're using the correct realm (`wallet-realm`, not `master`)

### Service Communication Issues

**Problem:** Services cannot communicate via RabbitMQ.

**Solutions:**
1. Verify RabbitMQ is running: `docker-compose ps rabbitmq`
2. Check RabbitMQ Management UI: http://localhost:15672
3. Verify queues exist in RabbitMQ UI
4. Check service logs for connection errors: `docker-compose logs <service-name> | grep -i rabbit`

### Keycloak Realm Not Imported

**Problem:** `wallet-realm` is not available in Keycloak.

**Solutions:**
1. Check Keycloak logs for import errors: `docker-compose logs keycloak | grep -i realm`
2. Verify `wallet-realm.json` file exists in project root
3. Restart Keycloak: `docker-compose restart keycloak`
4. Manually import realm via Keycloak Admin Console if needed

### Port Already in Use

**Problem:** "Port is already allocated" error.

**Solutions:**
1. Find process using the port:
   ```bash
   # Windows
   netstat -ano | findstr :<port>
   
   # Mac/Linux
   lsof -i :<port>
   ```
2. Stop the conflicting process or change the port in docker-compose.yml
3. Restart Docker Desktop if needed

### Vault Connection Issues

**Problem:** Services cannot connect to Vault.

**Solutions:**
1. Verify Vault is running: `docker-compose ps vault`
2. Check Vault UI: http://localhost:8200
3. Verify Vault token in service configuration: `my-root-token` (dev mode)
4. Check service logs for Vault errors: `docker-compose logs <service-name> | grep -i vault`

### Service Health Check Fails

**Problem:** Health check endpoint returns unhealthy.

**Solutions:**
1. Check service logs for errors: `docker-compose logs <service-name>`
2. Verify database connection
3. Verify RabbitMQ connection
4. Check for missing environment variables
5. Verify Keycloak connection and client secret

---

## Next Steps

Now that your environment is set up:

1. **Explore the Architecture:** Read [Architecture Documentation](architecture.md) to understand system design
2. **Learn About Services:** Check [Services Documentation](services.md) for API details
3. **Understand Patterns:** Review [Patterns and Practices](patterns-and-practices.md) for implementation details
4. **Configure Security:** See [Security Documentation](security.md) for authentication flows

---

## Additional Resources

- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [Keycloak Documentation](https://www.keycloak.org/documentation)
- [.NET 8 Documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)
