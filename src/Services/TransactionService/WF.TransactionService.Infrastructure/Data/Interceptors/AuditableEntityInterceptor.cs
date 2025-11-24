using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using WF.Shared.Contracts.Abstractions;
using WF.TransactionService.Domain.Entities;

namespace WF.TransactionService.Infrastructure.Data.Interceptors;

public class AuditableEntityInterceptor(ICurrentUserService currentUserService) : SaveChangesInterceptor
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            AuditChanges(eventData.Context);
        }

        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            AuditChanges(eventData.Context);
        }

        return ValueTask.FromResult(result);
    }

    private void AuditChanges(DbContext context)
    {
        var auditLogs = new List<AuditLog>();
        var userId = currentUserService.UserId;

        foreach (var entry in context.ChangeTracker.Entries())
        {
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
                    auditLog.NewValues = JsonSerializer.Serialize(GetPropertyValues(entry, entry.CurrentValues), JsonOptions);
                    break;

                case EntityState.Modified:
                    auditLog.Type = "UPDATE";
                    var changedProperties = GetChangedProperties(entry);
                    auditLog.OldValues = JsonSerializer.Serialize(changedProperties.OldValues, JsonOptions);
                    auditLog.NewValues = JsonSerializer.Serialize(changedProperties.NewValues, JsonOptions);
                    auditLog.AffectedColumns = string.Join(",", changedProperties.ChangedPropertyNames);
                    break;

                case EntityState.Deleted:
                    auditLog.Type = "DELETE";
                    auditLog.OldValues = JsonSerializer.Serialize(GetPropertyValues(entry, entry.OriginalValues), JsonOptions);
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

    private static string GetPrimaryKeyValue(EntityEntry entry)
    {
        var key = entry.Entity.GetType()
            .GetProperties()
            .FirstOrDefault(p => p.Name == "Id" || p.Name == "CorrelationId" || p.Name.EndsWith("Id"));

        if (key is not null)
        {
            var value = entry.Property(key.Name).CurrentValue ?? entry.Property(key.Name).OriginalValue;
            return value?.ToString() ?? string.Empty;
        }

        var primaryKey = entry.Metadata.FindPrimaryKey();
        if (primaryKey is not null)
        {
            var keyValues = primaryKey.Properties
                .Select(p => (entry.Property(p.Name).CurrentValue ?? entry.Property(p.Name).OriginalValue)?.ToString() ?? string.Empty)
                .ToArray();
            return string.Join(",", keyValues);
        }

        return string.Empty;
    }

    private static Dictionary<string, object?> GetPropertyValues(EntityEntry entry, PropertyValues propertyValues)
    {
        var values = new Dictionary<string, object?>();

        foreach (var property in entry.Properties)
        {
            if (property.Metadata.IsPrimaryKey())
            {
                continue;
            }

            values[property.Metadata.Name] = propertyValues[property.Metadata.Name];
        }

        return values;
    }

    private static (Dictionary<string, object?> OldValues, Dictionary<string, object?> NewValues, List<string> ChangedPropertyNames) GetChangedProperties(EntityEntry entry)
    {
        var oldValues = new Dictionary<string, object?>();
        var newValues = new Dictionary<string, object?>();
        var changedPropertyNames = new List<string>();

        foreach (var property in entry.Properties)
        {
            if (property.Metadata.IsPrimaryKey() || !property.IsModified)
            {
                continue;
            }

            var propertyName = property.Metadata.Name;
            oldValues[propertyName] = property.OriginalValue;
            newValues[propertyName] = property.CurrentValue;
            changedPropertyNames.Add(propertyName);
        }

        return (oldValues, newValues, changedPropertyNames);
    }
}

