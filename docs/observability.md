# Observability

This document provides comprehensive information about observability and monitoring in the Wallet Framework. It covers distributed tracing, structured logging, metrics collection, and audit logging.

## Table of Contents

1. [Dashboard Access](#dashboard-access)
2. [Distributed Tracing with Jaeger](#distributed-tracing-with-jaeger)
3. [Structured Logging with Serilog](#structured-logging-with-serilog)
4. [Metrics with Prometheus and Grafana](#metrics-with-prometheus-and-grafana)
5. [Audit Logs](#audit-logs)

---

## Dashboard Access

### Monitoring Tools Overview

The Wallet Framework provides several monitoring and observability tools accessible via web interfaces:

| Tool | URL | Default Credentials | Purpose |
|------|-----|---------------------|---------|
| **Jaeger UI** | http://localhost:16686 | None | Distributed tracing visualization |
| **Grafana** | http://localhost:3000 | admin/admin | Metrics dashboards and visualization |
| **Prometheus** | http://localhost:9090 | None | Metrics collection and querying |
| **RabbitMQ Management** | http://localhost:15672 | user/password | Message broker monitoring |
| **PgAdmin** | http://localhost:5050 | admin@example.com/admin | PostgreSQL database GUI |

### Port Configuration

From [docker-compose.yml](docker-compose.yml):

- **Jaeger**: 
  - UI: Port `16686`
  - OTLP gRPC: Port `4317`
  - OTLP HTTP: Port `4318`
  - Agent: Port `6831/udp`
- **Prometheus**: Port `9090`
- **Grafana**: Port `3000`
- **RabbitMQ Management**: Port `15672`

### Quick Access Guide

1. **Start the observability stack:**
   ```bash
   docker-compose up -d prometheus grafana jaeger rabbitmq
   ```

2. **Access dashboards:**
   - Open Jaeger UI: http://localhost:16686
   - Open Grafana: http://localhost:3000 (login with admin/admin)
   - Open Prometheus: http://localhost:9090
   - Open RabbitMQ Management: http://localhost:15672 (login with user/password)

---

## Distributed Tracing with Jaeger

### Overview

The Wallet Framework uses **OpenTelemetry** for distributed tracing, sending traces to **Jaeger** via the OTLP (OpenTelemetry Protocol) exporter. This allows you to track requests as they flow through multiple microservices.

### OpenTelemetry Configuration

OpenTelemetry is configured in [OpenTelemetryExtensions.cs](src/Services/CustomerService/WF.CustomerService.Api/Extensions/OpenTelemetryExtensions.cs):

```csharp
public static IServiceCollection AddOpenTelemetry(this IServiceCollection services, string serviceName, string version)
{
    services.AddOpenTelemetry()
        .ConfigureResource(resource =>
        {
            var attributes = OpenTelemetryConfig.GetResourceAttributes(serviceName, version);
            resource.AddAttributes(
                attributes.Select(kv => new KeyValuePair<string, object>(kv.Key, kv.Value))
            );
        })
        .WithTracing(tracing =>
        {
            // Add common activity sources
            foreach (var source in OpenTelemetryConfig.CommonActivitySources)
            {
                tracing.AddSource(source);
            }
            
            // Add service-specific source
            tracing.AddSource($"WF.{serviceName}");
            
            // Instrumentation
            tracing.AddAspNetCoreInstrumentation();      // HTTP requests
            tracing.AddHttpClientInstrumentation();       // Outgoing HTTP calls
            tracing.AddEntityFrameworkCoreInstrumentation(); // Database queries
            
            // Export to Jaeger via OTLP
            tracing.AddOtlpExporter(opts =>
            {
                opts.Endpoint = new Uri(OpenTelemetryConfig.OtlpEndpoint);
            });
        })
        .WithMetrics(metrics => 
        {
            metrics.AddAspNetCoreInstrumentation();
            metrics.AddHttpClientInstrumentation();
            metrics.AddRuntimeInstrumentation();
            metrics.AddPrometheusExporter();
        });

    return services;
}
```

### Activity Sources

Common activity sources are defined in [OpenTelemetryConfig.cs](shared/WF.Shared.Observability/OpenTelemetryConfig.cs):

```csharp
public static string[] CommonActivitySources => new[]
{
    "MassTransit",              // Message bus
    "WF.CustomerService",
    "WF.FraudService",
    "WF.TransactionService",
    "WF.WalletService"
};
```

### TraceId Correlation

Every request generates a unique **TraceId** that propagates across all services. This TraceId is:

1. **Included in logs** via Serilog enrichers
2. **Propagated via HTTP headers** automatically
3. **Included in MassTransit messages** for event correlation

### Using Jaeger UI to Track Requests

#### Step 1: Find the TraceId

When a request is made, check the application logs. The TraceId appears in log entries:

```
[10:30:45 INF] [abc123def4567890] [span123] Processing transfer request {TransactionId=guid-123}
```

The TraceId is: `abc123def4567890`

#### Step 2: Search in Jaeger UI

1. Open Jaeger UI: http://localhost:16686
2. In the search panel:
   - Select the **Service** (e.g., `WF.TransactionService`)
   - Enter the **TraceId** in the search box
   - Click **Find Traces**

#### Step 3: Analyze the Trace

The trace view shows:
- **Timeline**: Visual representation of spans across services
- **Span Details**: Click any span to see:
  - Operation name
  - Duration
  - Tags (HTTP method, status code, etc.)
  - Logs (if any)
- **Service Map**: Visual representation of service dependencies

#### Example Trace Flow

For a P2P transfer request, you might see:

```
WF.ApiGateway (HTTP POST /api/v1/transactions)
  └─ WF.TransactionService (CreateTransferCommand)
      ├─ WF.FraudService (CheckFraudCommand) [HTTP call]
      ├─ WF.CustomerService (GetCustomerByIdentity) [HTTP call]
      ├─ WF.WalletService (DebitSenderWalletCommand) [RabbitMQ]
      └─ WF.WalletService (CreditReceiverWalletCommand) [RabbitMQ]
```

### Trace Propagation

Traces automatically propagate via:
- **HTTP Headers**: `traceparent` header (W3C Trace Context)
- **MassTransit**: Trace context included in message headers
- **Database Queries**: EF Core instrumentation captures query spans

### Filtering Traces

In Jaeger UI, you can filter traces by:
- **Service**: Select specific microservice
- **Operation**: Filter by operation name (e.g., `GET /api/v1/customers`)
- **Tags**: Filter by tags (e.g., `http.status_code=200`)
- **Duration**: Filter by trace duration
- **Time Range**: Select time window

---

## Structured Logging with Serilog

### Overview

The Wallet Framework uses **Serilog** for structured logging with automatic trace correlation. Logs are enriched with TraceId, SpanId, and other contextual information.

### Configuration

Serilog is configured in [LoggingDIExtensions.cs](src/Services/WalletService/WF.WalletService.Api/Logging/LoggingDIExtensions.cs):

```csharp
public static IHostBuilder UseLogging(this IHostBuilder builder)
{
    builder.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithThreadId()
            .Enrich.WithProperty("Application", context.HostingEnvironment.ApplicationName)
            .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
            .Enrich.WithTraceId()  // Custom enricher
            .Enrich.WithSpanId();  // Custom enricher
    });

    return builder;
}
```

### Log Sinks

From [appsettings.Development.json](src/Services/WalletService/WF.WalletService.Api/appsettings.Development.json):

**Console Sink:**
- Colored output for development
- Output template includes TraceId and SpanId

**File Sink:**
- Rolling daily logs
- Retention: 7 days
- Path: `logs/log-{Date}.txt`

### Log Output Format

**Console Output:**
```
[10:30:45 INF] [abc123def456] [span789] Processing transfer request {TransactionId=guid-123} {Properties:j}
```

**File Output:**
```
[2024-01-15 10:30:45.123 +00:00 INF] [abc123def456] [span789] Processing transfer request {TransactionId=guid-123} {Properties:j}
```

### Custom Enrichers

TraceId and SpanId are extracted from `System.Diagnostics.Activity`:

```csharp
internal class TraceIdEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var activity = Activity.Current;
        if (activity != null)
        {
            var traceId = activity.TraceId.ToString();
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("TraceId", traceId));
        }
    }
}

internal class SpanIdEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var activity = Activity.Current;
        if (activity != null)
        {
            var spanId = activity.SpanId.ToString();
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("SpanId", spanId));
        }
    }
}
```

### Logging Best Practices

**DO:**
- Use message templates (not string interpolation)
- Include structured properties
- Use appropriate log levels (Information, Warning, Error)

**Example (Correct):**
```csharp
_logger.LogInformation("Processing transfer {TransactionId} for customer {CustomerId}", 
    transactionId, customerId);
```

**Example (Incorrect):**
```csharp
_logger.LogInformation($"Processing transfer {transactionId} for customer {customerId}");
```

**Log Levels:**
- **Information**: Normal application flow
- **Warning**: Unexpected but handled situations
- **Error**: Exceptions and failures
- **Debug**: Detailed diagnostic information (development only)

### Log Enrichment

Logs are automatically enriched with:
- **TraceId**: Distributed trace identifier
- **SpanId**: Current span identifier
- **MachineName**: Server name
- **EnvironmentName**: ASPNETCORE_ENVIRONMENT
- **ThreadId**: Thread identifier
- **Application**: Service name
- **Properties**: Additional context from `LogContext.PushProperty()`

---

## Metrics with Prometheus and Grafana

### Prometheus Configuration

Prometheus is configured in [prometheus.yml](prometheus.yml) to scrape metrics from all services:

```yaml
global:
  scrape_interval: 15s

scrape_configs:
  - job_name: "prometheus"
    static_configs:
      - targets: ["localhost:9090"]
  
  - job_name: "apigateway"
    static_configs:
      - targets: ["apigateway:8080"]
    metrics_path: "/metrics"
  
  - job_name: "customer-service"
    static_configs:
      - targets: ["customerservice:8080"]
    metrics_path: "/metrics"
  
  - job_name: "fraud-service"
    static_configs:
      - targets: ["fraudservice:8080"]
    metrics_path: "/metrics"
  
  - job_name: "transaction-service"
    static_configs:
      - targets: ["transactionservice:8080"]
    metrics_path: "/metrics"
  
  - job_name: "wallet-service"
    static_configs:
      - targets: ["walletservice:8080"]
    metrics_path: "/metrics"
```

### Metrics Exposed

Each service exposes metrics via the `/metrics` endpoint (Prometheus scraping endpoint):

**HTTP Metrics:**
- `http_server_request_duration_seconds` - Request duration histogram
- `http_server_requests_total` - Total request count
- `http_server_active_requests` - Active request gauge

**Runtime Metrics:**
- `dotnet_gc_collections_total` - GC collection count
- `dotnet_gc_heap_size_bytes` - Heap size
- `dotnet_thread_pool_threads` - Thread pool threads
- `dotnet_process_cpu_usage` - CPU usage

**Database Metrics (EF Core):**
- `efcore_active_db_contexts` - Active DbContext instances
- `efcore_queries_total` - Total query count

### Accessing Metrics

**Prometheus UI:**
1. Open http://localhost:9090
2. Navigate to **Status > Targets** to verify all services are scraped
3. Use **Graph** tab to query metrics:
   ```
   rate(http_server_requests_total[5m])
   ```

**Service Endpoints:**
- CustomerService: http://localhost:7001/metrics
- WalletService: http://localhost:7004/metrics
- TransactionService: http://localhost:7003/metrics
- FraudService: http://localhost:7002/metrics
- ApiGateway: http://localhost:5000/metrics

### Grafana Configuration

Grafana is pre-configured with Prometheus as the default datasource in [datasource.yml](grafana/provisioning/datasources/datasource.yml):

```yaml
apiVersion: 1

datasources:
  - name: Prometheus
    type: prometheus
    access: proxy
    url: http://prometheus:9090
    isDefault: true
    editable: true
```

### Importing Dashboards

#### Option 1: Import from Grafana.com

1. Open Grafana: http://localhost:3000
2. Login with `admin/admin`
3. Navigate to **Dashboards > Import**
4. Enter dashboard ID:
   - **ASP.NET Core**: `10915`
   - **.NET Runtime**: `10427`
5. Select **Prometheus** datasource
6. Click **Import**

#### Option 2: Create Custom Dashboard

1. Navigate to **Dashboards > New Dashboard**
2. Add panels for:
   - Request rate: `rate(http_server_requests_total[5m])`
   - Request duration (p95): `histogram_quantile(0.95, rate(http_server_request_duration_seconds_bucket[5m]))`
   - Error rate: `rate(http_server_requests_total{status_code=~"5.."}[5m])`
   - Memory usage: `dotnet_gc_heap_size_bytes`
   - CPU usage: `rate(dotnet_process_cpu_usage[5m])`

### Recommended Dashboards

**Service-Level Dashboard:**
- Request rate per service
- Request duration (p50, p95, p99)
- Error rate (4xx, 5xx)
- Active requests

**System-Level Dashboard:**
- Total request rate
- Average response time
- Error percentage
- Memory usage
- CPU usage

---

## Audit Logs

### Overview

The Wallet Framework automatically tracks all database changes (INSERT, UPDATE, DELETE) in an `AuditLog` table. This provides a complete audit trail of who changed what and when.

### AuditLog Entity Structure

The `AuditLog` entity is defined in [AuditLog.cs](src/Services/WalletService/WF.WalletService.Domain/Entities/AuditLog.cs):

```csharp
public class AuditLog
{
    public Guid Id { get; set; }
    public string? UserId { get; set; }           // Who made the change
    public string Type { get; set; } = string.Empty;  // INSERT, UPDATE, DELETE
    public string TableName { get; set; } = string.Empty;  // Affected table
    public DateTime DateTimeUtc { get; set; }     // When the change occurred
    public string? OldValues { get; set; }        // JSON of previous values (UPDATE/DELETE)
    public string? NewValues { get; set; }        // JSON of new values (INSERT/UPDATE)
    public string? AffectedColumns { get; set; }  // Comma-separated column names (UPDATE)
    public string PrimaryKey { get; set; } = string.Empty;  // Entity primary key value
}
```

### Database Schema

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| `Id` | `uuid` | No | Primary key |
| `UserId` | `varchar` | Yes | User ID who made the change (from `ICurrentUserService`) |
| `Type` | `varchar` | No | Operation type: `INSERT`, `UPDATE`, `DELETE` |
| `TableName` | `varchar` | No | Name of the affected table |
| `DateTimeUtc` | `timestamp` | No | UTC timestamp of the change |
| `OldValues` | `text` | Yes | JSON object with old values (UPDATE/DELETE only) |
| `NewValues` | `text` | Yes | JSON object with new values (INSERT/UPDATE only) |
| `AffectedColumns` | `varchar` | Yes | Comma-separated list of changed columns (UPDATE only) |
| `PrimaryKey` | `varchar` | No | Primary key value of the affected entity |

### AuditableEntityInterceptor

The audit logging is implemented via an EF Core interceptor in [AuditableEntityInterceptor.cs](src/Services/WalletService/WF.WalletService.Infrastructure/Data/Interceptors/AuditableEntityInterceptor.cs):

```csharp
public class AuditableEntityInterceptor(ICurrentUserService currentUserService) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            AuditChanges(eventData.Context);
        }
        return result;
    }

    private void AuditChanges(DbContext context)
    {
        var auditLogs = new List<AuditLog>();
        var userId = currentUserService.UserId;  // Get current user

        foreach (var entry in context.ChangeTracker.Entries())
        {
            // Skip AuditLog entries to prevent recursion
            if (entry.Entity is AuditLog)
            {
                continue;
            }

            var entityType = entry.Entity.GetType();
            var tableName = context.Model.FindEntityType(entityType)?.GetTableName() ?? entityType.Name;
            var primaryKey = GetPrimaryKeyValue(entry);

            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TableName = tableName,
                PrimaryKey = primaryKey,
                DateTimeUtc = DateTime.UtcNow
            };

            switch (entry.State)
            {
                case EntityState.Added:
                    auditLog.Type = "INSERT";
                    auditLog.NewValues = JsonSerializer.Serialize(GetPropertyValues(entry, entry.CurrentValues));
                    break;

                case EntityState.Modified:
                    auditLog.Type = "UPDATE";
                    var changedProperties = GetChangedProperties(entry);
                    auditLog.OldValues = JsonSerializer.Serialize(changedProperties.OldValues);
                    auditLog.NewValues = JsonSerializer.Serialize(changedProperties.NewValues);
                    auditLog.AffectedColumns = string.Join(",", changedProperties.ChangedPropertyNames);
                    break;

                case EntityState.Deleted:
                    auditLog.Type = "DELETE";
                    auditLog.OldValues = JsonSerializer.Serialize(GetPropertyValues(entry, entry.OriginalValues));
                    break;
            }

            if (entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            {
                auditLogs.Add(auditLog);
            }
        }

        if (auditLogs.Count > 0)
        {
            context.Set<AuditLog>().AddRange(auditLogs);
        }
    }
}
```

### Registration

The interceptor is registered in [DependencyInjectionExtensions.cs](src/Services/WalletService/WF.WalletService.Infrastructure/DependencyInjectionExtensions.cs):

```csharp
services.AddScoped<AuditableEntityInterceptor>();

services.AddDbContext<WalletDbContext>((serviceProvider, options) =>
{
    options.UseNpgsql(connectionString);
    options.AddInterceptors(serviceProvider.GetRequiredService<AuditableEntityInterceptor>());
});
```

### How It Works

1. **Before SaveChanges**: The interceptor intercepts `SaveChanges`/`SaveChangesAsync`
2. **Entity Inspection**: Iterates through all tracked entities in `ChangeTracker`
3. **Change Detection**: Identifies INSERT, UPDATE, DELETE operations
4. **Value Capture**:
   - **INSERT**: Captures all property values (NewValues)
   - **UPDATE**: Captures only changed properties (OldValues and NewValues)
   - **DELETE**: Captures all property values (OldValues)
5. **Audit Log Creation**: Creates `AuditLog` entries
6. **Automatic Persistence**: Audit logs are saved in the same transaction

### Querying Audit Logs

#### Find All Changes to a Specific Entity

```sql
SELECT 
    "Id",
    "UserId",
    "Type",
    "TableName",
    "PrimaryKey",
    "DateTimeUtc",
    "OldValues",
    "NewValues",
    "AffectedColumns"
FROM "AuditLogs"
WHERE "TableName" = 'Wallets'
  AND "PrimaryKey" = 'wallet-guid-here'
ORDER BY "DateTimeUtc" DESC;
```

#### Find All Changes by a User

```sql
SELECT 
    "Type",
    "TableName",
    "PrimaryKey",
    "DateTimeUtc",
    "NewValues"
FROM "AuditLogs"
WHERE "UserId" = 'user-guid-here'
ORDER BY "DateTimeUtc" DESC
LIMIT 100;
```

#### Find All Updates to a Specific Table

```sql
SELECT 
    "UserId",
    "PrimaryKey",
    "DateTimeUtc",
    "AffectedColumns",
    "OldValues",
    "NewValues"
FROM "AuditLogs"
WHERE "TableName" = 'Customers'
  AND "Type" = 'UPDATE'
ORDER BY "DateTimeUtc" DESC;
```

#### Find Recent Changes (Last 24 Hours)

```sql
SELECT 
    "Type",
    "TableName",
    "PrimaryKey",
    "UserId",
    "DateTimeUtc"
FROM "AuditLogs"
WHERE "DateTimeUtc" >= NOW() - INTERVAL '24 hours'
ORDER BY "DateTimeUtc" DESC;
```

### Audit Log Example

**INSERT Operation:**
```json
{
  "Id": "guid-1",
  "UserId": "user-123",
  "Type": "INSERT",
  "TableName": "Wallets",
  "PrimaryKey": "wallet-456",
  "DateTimeUtc": "2024-01-15T10:30:45Z",
  "NewValues": "{\"CustomerId\":\"customer-789\",\"Balance\":1000.00,\"Currency\":\"USD\"}",
  "OldValues": null,
  "AffectedColumns": null
}
```

**UPDATE Operation:**
```json
{
  "Id": "guid-2",
  "UserId": "user-123",
  "Type": "UPDATE",
  "TableName": "Wallets",
  "PrimaryKey": "wallet-456",
  "DateTimeUtc": "2024-01-15T10:35:20Z",
  "NewValues": "{\"Balance\":1500.00}",
  "OldValues": "{\"Balance\":1000.00}",
  "AffectedColumns": "Balance"
}
```

**DELETE Operation:**
```json
{
  "Id": "guid-3",
  "UserId": "user-123",
  "Type": "DELETE",
  "TableName": "Wallets",
  "PrimaryKey": "wallet-456",
  "DateTimeUtc": "2024-01-15T10:40:10Z",
  "NewValues": null,
  "OldValues": "{\"CustomerId\":\"customer-789\",\"Balance\":1500.00,\"Currency\":\"USD\"}",
  "AffectedColumns": null
}
```

### Best Practices

1. **Performance**: Audit logs are written synchronously in the same transaction. For high-throughput scenarios, consider async audit logging.
2. **Storage**: Audit logs can grow large. Implement retention policies or archival strategies.
3. **Privacy**: Ensure sensitive data is not logged in audit entries (e.g., passwords, credit card numbers).
4. **Querying**: Create indexes on frequently queried columns:
   ```sql
   CREATE INDEX idx_auditlogs_tablename_pk ON "AuditLogs"("TableName", "PrimaryKey");
   CREATE INDEX idx_auditlogs_userid ON "AuditLogs"("UserId");
   CREATE INDEX idx_auditlogs_datetime ON "AuditLogs"("DateTimeUtc");
   ```

---

## Additional Resources

- [Getting Started Guide](getting-started.md) - Initial setup
- [Architecture Documentation](architecture.md) - System design overview
- [OpenTelemetry Documentation](https://opentelemetry.io/docs/)
- [Jaeger Documentation](https://www.jaegertracing.io/docs/)
- [Prometheus Documentation](https://prometheus.io/docs/)
- [Grafana Documentation](https://grafana.com/docs/)
- [Serilog Documentation](https://serilog.net/)
