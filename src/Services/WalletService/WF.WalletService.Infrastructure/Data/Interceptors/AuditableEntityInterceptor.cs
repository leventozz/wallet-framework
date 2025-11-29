using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using WF.Shared.Contracts.Abstractions;
using WF.WalletService.Domain.Entities;

namespace WF.WalletService.Infrastructure.Data.Interceptors;

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

            var typeName = entry.Entity.GetType().Name;
            if (typeName is "InboxState" or "OutboxMessage" or "OutboxState")
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

            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();
            var changedColumns = new List<string>();

            ExtractChanges(entry.Properties, entry.ComplexProperties, oldValues, newValues, changedColumns, entry.State);

            switch (entry.State)
            {
                case EntityState.Added:
                    auditLog.Type = "INSERT";
                    auditLog.NewValues = JsonSerializer.Serialize(newValues, JsonOptions);
                    break;

                case EntityState.Modified:
                    if (changedColumns.Count == 0) continue; // Skip if no actual changes
                    auditLog.Type = "UPDATE";
                    auditLog.OldValues = JsonSerializer.Serialize(oldValues, JsonOptions);
                    auditLog.NewValues = JsonSerializer.Serialize(newValues, JsonOptions);
                    auditLog.AffectedColumns = string.Join(",", changedColumns);
                    break;

                case EntityState.Deleted:
                    auditLog.Type = "DELETE";
                    auditLog.OldValues = JsonSerializer.Serialize(oldValues, JsonOptions);
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

    private static void ExtractChanges(
        IEnumerable<PropertyEntry> properties,
        IEnumerable<ComplexPropertyEntry> complexProperties,
        Dictionary<string, object?> oldValues,
        Dictionary<string, object?> newValues,
        List<string> changedColumns,
        EntityState state,
        string prefix = "")
    {
        foreach (var property in properties)
        {
            if (property.Metadata.IsPrimaryKey())
            {
                continue;
            }

            var propertyName = string.IsNullOrEmpty(prefix) ? property.Metadata.Name : $"{prefix}.{property.Metadata.Name}";
            var originalValue = property.OriginalValue;
            var currentValue = property.CurrentValue;

            switch (state)
            {
                case EntityState.Added:
                    newValues[propertyName] = currentValue;
                    break;

                case EntityState.Deleted:
                    oldValues[propertyName] = originalValue;
                    break;

                case EntityState.Modified:
                    if (property.IsModified && !Equals(originalValue, currentValue))
                    {
                        oldValues[propertyName] = originalValue;
                        newValues[propertyName] = currentValue;
                        changedColumns.Add(propertyName);
                    }
                    break;
            }
        }

        foreach (var complexProperty in complexProperties)
        {
            var complexPrefix = string.IsNullOrEmpty(prefix)
                ? complexProperty.Metadata.Name
                : $"{prefix}.{complexProperty.Metadata.Name}";

            ExtractChanges(
                complexProperty.Properties,
                complexProperty.ComplexProperties,
                oldValues,
                newValues,
                changedColumns,
                state,
                complexPrefix);
        }
    }
}

