# Patterns and Practices

This document serves as the "how we do things here" guide for developers contributing to the Wallet Framework project. It explains the coding standards, design patterns, and conventions used throughout the codebase.

## Table of Contents

1. [Result Pattern](#result-pattern)
2. [Value Objects](#value-objects)
3. [CQRS (Command Query Responsibility Segregation)](#cqrs-command-query-responsibility-segregation)
4. [Validation with FluentValidation](#validation-with-fluentvalidation)
5. [Project Directory Structure](#project-directory-structure)

---

## Result Pattern

### Why We Don't Throw Exceptions

In the Wallet Framework, we use the **Result Pattern** instead of throwing exceptions for business logic failures. This approach provides several benefits:

- **Exceptions are for exceptional cases**: Exceptions should be reserved for truly exceptional situations (system failures, null references, etc.), not for expected business rule violations
- **Type-safe error handling**: The Result pattern makes errors explicit in the method signature, forcing callers to handle them
- **Better flow control**: Early returns with Result values make code more readable and maintainable
- **No performance overhead**: Unlike exceptions, Result pattern doesn't have stack unwinding overhead
- **Composable**: Results can be easily chained and combined

### Implementation

The Result pattern is implemented in [Result.cs](shared/WF.Shared.Contracts/Result/Result.cs) and [Error.cs](shared/WF.Shared.Contracts/Result/Error.cs):

```csharp
// Result for void operations
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }
    
    protected Result(bool isSuccess, Error error)
    {
        // Validation: success must have Error.None, failure must have an error
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException();
        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException();
            
        IsSuccess = isSuccess;
        Error = error;
    }
    
    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
}

// Result<T> for operations returning a value
public class Result<T> : Result
{
    private readonly T? _value;
    
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result can not be accessed.");
    
    protected internal Result(T? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }
    
    public static Result<T> Success(T value) => new(value, true, Error.None);
    public static new Result<T> Failure(Error error) => new(default, false, error);
    
    public static Result<T> Create(T? value)
    {
        return value is not null
            ? Success(value)
            : Failure(Error.NullValue);
    }
}
```

### Error Types

Errors are represented as immutable records with a code and message:

```csharp
public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "The specified result value is null.");
    
    // Factory methods for common error types
    public static Error NotFound(string name, object id) =>
        new("NotFound", $"{name} with id '{id}' was not found.");
    
    public static Error Validation(string code, string message) =>
        new(code, message);
    
    public static Error Conflict(string code, string message) =>
        new(code, message);
    
    public static Error Failure(string code, string message) =>
        new(code, message);
}
```

### Usage Examples

Here's how the Result pattern is used in practice, from [CreateCustomerCommandHandler.cs](src/Services/CustomerService/WF.CustomerService.Application/Features/Customers/Commands/CreateCustomer/CreateCustomerCommandHandler.cs):

```csharp
public async Task<Result<Guid>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
{
    // Create value objects with validation
    var nameResult = PersonName.Create(request.FirstName, request.LastName);
    if (nameResult.IsFailure)
    {
        _logger.LogWarning("Failed to create person name: {Error}", nameResult.Error.Message);
        return Result<Guid>.Failure(nameResult.Error);  // Early return on failure
    }
    
    var emailResult = Email.Create(request.Email);
    if (emailResult.IsFailure)
    {
        _logger.LogWarning("Failed to create email: {Error}", emailResult.Error.Message);
        return Result<Guid>.Failure(emailResult.Error);
    }
    
    // Use the value from successful result
    var customerResult = Customer.Create(
        identityId, 
        nameResult.Value,  // Extract value from Result
        emailResult.Value, 
        customerNumber, 
        phoneNumberResult.Value);
        
    if (customerResult.IsFailure)
    {
        return Result<Guid>.Failure(customerResult.Error);
    }
    
    // Success path
    var customer = customerResult.Value;
    await _customerRepository.AddCustomerAsync(customer);
    await _unitOfWork.SaveChangesAsync(cancellationToken);
    
    return Result<Guid>.Success(customer.Id);
}
```

**Key Points:**
- Always check `IsFailure` before accessing `Value`
- Use early returns to avoid deep nesting
- Propagate errors by returning `Result.Failure(error)`
- Log failures appropriately before returning

---

## Value Objects

### Why Not Primitives?

Value Objects are used throughout the domain to represent domain concepts with validation and behavior. Benefits include:

- **Type safety**: Prevents primitive obsession (e.g., `string email` vs `Email email`)
- **Self-validating**: Validation logic is encapsulated in the value object
- **Immutability**: `readonly record struct` ensures values cannot be modified after creation
- **Domain semantics**: Makes the code more expressive and self-documenting
- **Encapsulated behavior**: Methods and operators can be defined on value objects

### Value Objects in the Project

| Service | Value Objects | Purpose |
|---------|--------------|---------|
| **CustomerService** | `Email`, `PersonName`, `PhoneNumber` | Customer identity and contact information |
| **WalletService** | `Money`, `Iban` | Financial amounts and bank account identifiers |
| **FraudService** | `IpAddress`, `Money`, `TimeRange` | Fraud detection data |

### Standard Implementation Pattern

All value objects follow a consistent pattern using `readonly record struct`:

```csharp
public readonly record struct Money
{
    public decimal Amount { get; }
    public string Currency { get; }
    
    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }
    
    // Factory method with validation
    public static Result<Money> Create(decimal amount, string currency)
    {
        if (amount < 0)
            return Result<Money>.Failure(
                Error.Validation("Money.NegativeAmount", "Money amount cannot be negative."));
        
        if (string.IsNullOrWhiteSpace(currency))
            return Result<Money>.Failure(
                Error.Validation("Money.InvalidCurrency", "Currency cannot be null or empty."));
        
        return Result<Money>.Success(new Money(amount, currency.Trim().ToUpperInvariant()));
    }
    
    // Operator overloads for domain behavior
    public static Money operator +(Money left, Money right)
    {
        ValidateSameCurrency(left, right);
        return new Money(left.Amount + right.Amount, left.Currency);
    }
    
    public static Money operator -(Money left, Money right)
    {
        ValidateSameCurrency(left, right);
        var result = left.Amount - right.Amount;
        if (result < 0)
            throw new InvalidOperationException("Result of subtraction cannot be negative.");
        return new Money(result, left.Currency);
    }
    
    // Comparison operators
    public static bool operator <(Money left, Money right) { ... }
    public static bool operator >(Money left, Money right) { ... }
    
    private static void ValidateSameCurrency(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException(
                $"Cannot operate money with different currencies: {left.Currency} and {right.Currency}.");
    }
    
    public override string ToString() => $"{Amount:F2} {Currency}";
}
```

**Key Characteristics:**
- **Private constructor**: Forces creation through the `Create` factory method
- **Static `Create` method**: Returns `Result<T>` for validation
- **Static `FromDatabaseValue` method**: For EF Core mapping (bypasses validation, assumes data is valid)
- **Immutability**: `readonly record struct` ensures values cannot be modified
- **Operator overloads**: Domain-specific behavior (e.g., money arithmetic)

### Example: Email Value Object

From [Email.cs](src/Services/CustomerService/WF.CustomerService.Domain/ValueObjects/Email.cs):

```csharp
public readonly record struct Email
{
    private const int MaxEmailLength = 320;
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase,
        TimeSpan.FromMilliseconds(250));
    
    public string Value { get; }
    
    private Email(string value)
    {
        Value = value;
    }
    
    public static Result<Email> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<Email>.Failure(
                Error.Validation("Email.Required", "Email cannot be null or empty."));
        
        var trimmedValue = value.Trim();
        
        if (trimmedValue.Length > MaxEmailLength)
            return Result<Email>.Failure(
                Error.Validation("Email.MaxLength", $"Email must not exceed {MaxEmailLength} characters."));
        
        if (!EmailRegex.IsMatch(trimmedValue))
            return Result<Email>.Failure(
                Error.Validation("Email.InvalidFormat", "Email must be a valid email address."));
        
        // Additional validation using MailAddress
        try
        {
            var mailAddress = new MailAddress(trimmedValue);
            if (mailAddress.Address != trimmedValue)
                return Result<Email>.Failure(
                    Error.Validation("Email.InvalidCharacters", "Email contains invalid characters."));
        }
        catch (Exception ex) when (ex is ArgumentException || ex is FormatException)
        {
            return Result<Email>.Failure(
                Error.Validation("Email.InvalidFormat", "Email must be a valid email address."));
        }
        
        return Result<Email>.Success(new Email(trimmedValue));
    }
    
    // For EF Core mapping (assumes database value is valid)
    public static Email FromDatabaseValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException("Email cannot be null or empty when reading from database.");
        return new Email(value.Trim());
    }
    
    public static implicit operator string(Email email) => email.Value;
    public override string ToString() => Value;
}
```

### Example: Iban Value Object

From [Iban.cs](src/Services/WalletService/WF.WalletService.Domain/ValueObjects/Iban.cs):

The `Iban` value object demonstrates more complex validation including:
- Format validation (regex)
- Length validation
- Check digit validation (Mod-97 algorithm)
- Normalization (removes spaces, converts to uppercase)

---

## CQRS (Command Query Responsibility Segregation)

### Overview

The project implements CQRS to separate read and write operations, optimizing each for its specific purpose:

| Concern | Technology | Pattern | Purpose |
|---------|-----------|---------|---------|
| **Write (Commands)** | EF Core 8 | Repository + Unit of Work | Domain behavior, transactions, event publishing |
| **Read (Queries)** | Dapper | Query Services + DTOs | Optimized reads, direct DTO projection |

### Write Side: EF Core + Repository

Command handlers use Entity Framework Core with the Repository pattern:

**Key Components:**
- `IRepository<T>`: Abstraction for aggregate access
- `IUnitOfWork`: Transaction management
- Domain entities: Rich domain models with behavior
- Outbox pattern: Reliable event publishing

**Example Flow:**

```csharp
public class CreateAccountAgeRuleCommandHandler(
    IAccountAgeRuleRepository _repository,
    IUnitOfWork _unitOfWork) 
    : IRequestHandler<CreateAccountAgeRuleCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateAccountAgeRuleCommand request, CancellationToken ct)
    {
        // 1. Create domain entity (with validation)
        var ruleResult = AccountAgeRule.Create(
            request.MinAccountAgeDays,
            request.MaxAllowedAmount,
            request.Description);
        
        if (ruleResult.IsFailure)
            return Result<Guid>.Failure(ruleResult.Error);
        
        // 2. Add to repository
        var rule = ruleResult.Value;
        await _repository.AddAsync(rule, ct);
        
        // 3. Commit transaction (includes Outbox pattern for events)
        await _unitOfWork.SaveChangesAsync(ct);
        
        // 4. Return success with created ID
        return Result<Guid>.Success(rule.Id);
    }
}
```

**Benefits:**
- Domain logic is encapsulated in entities
- Transaction boundaries are explicit
- Events are published atomically with data changes (Outbox pattern)
- Easy to test with repository abstractions

### Read Side: Dapper + Query Services

Query services use Dapper for optimized read operations:

**Key Characteristics:**
- `NpgsqlDataSource`: Connection management
- Raw SQL: Optimized queries with direct DTO projection
- No entity tracking: Reduced memory overhead
- Direct mapping: SQL results → DTOs

**Example from [CustomerQueryService.cs](src/Services/CustomerService/WF.CustomerService.Infrastructure/QueryServices/CustomerQueryService.cs):**

```csharp
public class CustomerQueryService(NpgsqlDataSource dataSource) : ICustomerQueryService
{
    public async Task<CustomerLookupDto?> GetCustomerByIdentityAsync(
        string identityId, 
        CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        
        const string sql = """
            SELECT "Id" AS "CustomerId", "CustomerNumber"
            FROM "Customers"
            WHERE "IdentityId" = @identityId 
                AND "IsActive" = true 
                AND "IsDeleted" = false;
            """;
        
        return await connection.QueryFirstOrDefaultAsync<CustomerLookupDto>(
            new CommandDefinition(sql, new { identityId }, cancellationToken: cancellationToken));
    }
}
```

**Benefits:**
- **Performance**: No entity tracking overhead, optimized SQL
- **Flexibility**: Can write complex queries with joins, aggregations
- **Direct projection**: SQL → DTOs without intermediate entities
- **Read models**: Can query denormalized read models for complex views

**Example with Complex Query from [AdminCustomerQueryService.cs](src/Services/CustomerService/WF.CustomerService.Infrastructure/QueryServices/AdminCustomerQueryService.cs):**

```csharp
public async Task<PagedResult<AdminCustomerListDto>> GetAllCustomersWithWalletsAsync(
    int pageNumber, 
    int pageSize, 
    CancellationToken cancellationToken)
{
    await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
    
    var offset = (pageNumber - 1) * pageSize;
    
    // Count query
    const string countSql = """
        SELECT COUNT(*) FROM "Customers" WHERE "IsDeleted" = false;
        """;
    var totalCount = await connection.ExecuteScalarAsync<int>(
        new CommandDefinition(countSql, cancellationToken: cancellationToken));
    
    // Data query with join
    const string sql = """
        SELECT 
            c."Id",
            c."CustomerNumber",
            CONCAT(c."FirstName", ' ', c."LastName") AS "FullName",
            c."Email",
            c."IsActive",
            c."CreatedAtUtc" AS "CreatedAt",
            w."Id" AS "WalletId",
            w."WalletNumber",
            w."Balance",
            w."Currency",
            w."State"
        FROM "Customers" c
        LEFT JOIN "WalletReadModels" w ON c."Id" = w."CustomerId"
        WHERE c."IsDeleted" = false
        ORDER BY c."CreatedAtUtc" DESC
        OFFSET @offset LIMIT @pageSize;
        """;
    
    // Multi-mapping for one-to-many relationship
    var customerDictionary = new Dictionary<Guid, AdminCustomerListDto>();
    
    await connection.QueryAsync<AdminCustomerListDto, AdminWalletDto?, AdminCustomerListDto>(
        new CommandDefinition(sql, new { offset, pageSize }, cancellationToken: cancellationToken),
        (customer, wallet) =>
        {
            if (!customerDictionary.TryGetValue(customer.Id, out var customerEntry))
            {
                customerEntry = customer;
                customerEntry.Wallets = new List<AdminWalletDto>();
                customerDictionary.Add(customer.Id, customerEntry);
            }
            if (wallet != null)
                customerEntry.Wallets.Add(wallet);
            return customerEntry;
        },
        splitOn: "WalletId");
    
    return new PagedResult<AdminCustomerListDto>(
        customerDictionary.Values.ToList(), 
        totalCount, 
        pageNumber, 
        pageSize);
}
```

---

## Validation with FluentValidation

### Pipeline Behavior Integration

Validation is handled automatically via MediatR pipeline behaviors. All validators are discovered and executed before command/query handlers.

**Implementation from [ValidationBehavior.cs](src/Services/CustomerService/WF.CustomerService.Application/Common/Behaviors/ValidationBehavior.cs):**

```csharp
public class ValidationBehavior<TRequest, TResponse>
    (IEnumerable<IValidator<TRequest>> _validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }
        
        var context = new ValidationContext<TRequest>(request);
        
        // Run all validators in parallel
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));
        
        var failures = validationResults
            .Where(r => r.Errors.Any())
            .SelectMany(r => r.Errors)
            .ToList();
        
        if (failures.Any())
        {
            throw new ValidationException(failures);
        }
        
        return await next();
    }
}
```

### Registration

Validators are registered automatically from the assembly:

```csharp
// In DependencyInjectionExtensions.cs
public static IServiceCollection AddApplication(this IServiceCollection services)
{
    services.AddMediatR(cfg =>
        cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
    
    // Auto-discover validators
    services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
    
    // Register validation behavior
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    
    return services;
}
```

### Domain vs Application Validation

The project uses a two-tier validation approach:

1. **Domain Validation** (Value Objects and Entities):
   - Encapsulated in `Create` factory methods
   - Returns `Result<T>` with domain errors
   - Examples: Email format, Money amount, PersonName rules

2. **Application Validation** (Commands/Queries):
   - FluentValidation for request DTOs
   - Validates input format, required fields, business rules
   - Throws `ValidationException` (caught by exception handler middleware)

**When to use which:**
- **Domain validation**: Always use for value objects and entity creation
- **FluentValidation**: Use for command/query DTOs to validate request structure and business rules before domain logic

---

## Project Directory Structure

### Service Layer Architecture

Each microservice follows Clean Architecture with clear layer separation:

```
src/Services/{ServiceName}/
├── WF.{ServiceName}.Api/                    # Presentation Layer
│   ├── Controllers/
│   │   ├── Base/                            # BaseController with common functionality
│   │   ├── Admin/                           # Admin-only endpoints
│   │   └── Internal/                        # Internal service-to-service endpoints
│   ├── Extensions/                          # Service registration extensions
│   │   ├── AuthenticationExtensions.cs
│   │   ├── ConfigurationExtensions.cs
│   │   └── OpenTelemetryExtensions.cs
│   ├── Middleware/                          # Custom middleware
│   │   └── ExceptionHandler.cs
│   └── Program.cs                           # Application entry point
│
├── WF.{ServiceName}.Application/            # Application Layer (CQRS)
│   ├── Common/
│   │   └── Behaviors/                       # MediatR pipeline behaviors
│   │       └── ValidationBehavior.cs
│   ├── Contracts/                           # Application contracts
│   │   └── DTOs/                           # Data Transfer Objects
│   ├── Features/                            # Feature-based organization
│   │   └── {Feature}/                      # e.g., Customers, Wallets
│   │       ├── Commands/
│   │       │   └── {CommandName}/          # e.g., CreateCustomer
│   │       │       ├── {CommandName}Command.cs
│   │       │       └── {CommandName}CommandHandler.cs
│   │       └── Queries/
│   │           └── {QueryName}/            # e.g., GetCustomerById
│   │               ├── {QueryName}Query.cs
│   │               └── {QueryName}QueryHandler.cs
│   └── DependencyInjectionExtensions.cs
│
├── WF.{ServiceName}.Domain/                 # Domain Layer
│   ├── Abstractions/                        # Repository interfaces
│   │   └── I{Entity}Repository.cs
│   ├── Entities/                            # Aggregate roots
│   │   └── {Entity}.cs
│   └── ValueObjects/                        # Value Objects
│       └── {ValueObject}.cs
│
└── WF.{ServiceName}.Infrastructure/         # Infrastructure Layer
    ├── Data/
    │   ├── {ServiceName}DbContext.cs       # EF Core DbContext
    │   └── UnitOfWork.cs                   # Transaction management
    ├── Migrations/                          # EF Core migrations
    ├── QueryServices/                       # Dapper read services
    │   └── {Entity}QueryService.cs
    ├── Repositories/                        # EF Core repository implementations
    │   └── {Entity}Repository.cs
    ├── EventBus/                            # MassTransit integration
    │   └── MassTransitEventPublisher.cs
    └── DependencyInjectionExtensions.cs
```

### Shared Projects

```
shared/
├── WF.Shared.Contracts/                     # Shared contracts across services
│   ├── Abstractions/                        # Shared interfaces
│   │   ├── IUnitOfWork.cs
│   │   ├── IIntegrationEventPublisher.cs
│   │   └── ICurrentUserService.cs
│   ├── Commands/                            # Inter-service command contracts
│   │   ├── Fraud/
│   │   └── Wallet/
│   ├── Dtos/                                # Shared DTOs
│   ├── Enums/                               # Shared enumerations
│   │   ├── Currency.cs
│   │   └── KycStatus.cs
│   ├── IntegrationEvents/                   # Event contracts
│   │   ├── Customer/
│   │   ├── Transaction/
│   │   └── Wallet/
│   └── Result/                              # Result pattern implementation
│       ├── Result.cs
│       ├── Error.cs
│       └── ResultExtensions.cs
│
└── WF.Shared.Observability/                 # OpenTelemetry configuration
    └── OpenTelemetryConfig.cs
```

### Naming Conventions

| Type | Convention | Example |
|------|------------|---------|
| **Commands** | `{Action}{Entity}Command` | `CreateCustomerCommand` |
| **Command Handlers** | `{Command}Handler` | `CreateCustomerCommandHandler` |
| **Queries** | `Get{Entity}Query` or `{Action}{Entity}Query` | `GetCustomerByIdQuery`, `GetAllCustomersQuery` |
| **Query Handlers** | `{Query}Handler` | `GetCustomerByIdQueryHandler` |
| **DTOs** | `{Entity}Dto` | `CustomerDto`, `CustomerLookupDto` |
| **Events** | `{Entity}{Action}Event` | `CustomerCreatedEvent`, `WalletDebitedEvent` |
| **Repositories (Interface)** | `I{Entity}Repository` | `ICustomerRepository` |
| **Repositories (Implementation)** | `{Entity}Repository` | `CustomerRepository` |
| **Query Services** | `{Entity}QueryService` | `CustomerQueryService` |
| **Value Objects** | PascalCase noun | `Email`, `Money`, `PersonName` |
| **Entities** | PascalCase noun | `Customer`, `Wallet`, `Transaction` |

### Feature-Based Organization

Commands and queries are organized by feature, not by type:

```
Features/
└── Customers/
    ├── Commands/
    │   ├── CreateCustomer/
    │   │   ├── CreateCustomerCommand.cs
    │   │   └── CreateCustomerCommandHandler.cs
    │   └── UpdateCustomer/
    │       ├── UpdateCustomerCommand.cs
    │       └── UpdateCustomerCommandHandler.cs
    └── Queries/
        ├── GetCustomerById/
        │   ├── GetCustomerByIdQuery.cs
        │   └── GetCustomerByIdQueryHandler.cs
        └── GetAllCustomers/
            ├── GetAllCustomersQuery.cs
            └── GetAllCustomersQueryHandler.cs
```

**Benefits:**
- Related code is co-located
- Easy to find all code for a feature
- Clear boundaries between features
- Supports feature flags and modular development

---

## Additional Resources

- [Architecture Documentation](architecture.md) - System design and patterns
- [Services Documentation](services.md) - Service responsibilities and APIs
- [Getting Started Guide](getting-started.md) - Setup and first steps
