namespace WF.TransactionService.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; set; }

    public string? UserId { get; set; } 

    public string Type { get; set; } = string.Empty; 

    public string TableName { get; set; } = string.Empty;

    public DateTime DateTimeUtc { get; set; }

    public string? OldValues { get; set; } 

    public string? NewValues { get; set; } 

    public string? AffectedColumns { get; set; } 

    public string PrimaryKey { get; set; } = string.Empty;
}

