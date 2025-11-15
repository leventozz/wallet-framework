namespace WF.FraudService.Domain.Entities;

public class BlockedIpRule
{
    public Guid Id { get; private set; }
    public string IpAddress { get; private set; } = string.Empty;
    public string? Reason { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ExpiresAtUtc { get; private set; }
    public bool IsActive { get; private set; }

    private BlockedIpRule() { }

    public BlockedIpRule(string ipAddress, string? reason = null, DateTime? expiresAtUtc = null)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            throw new ArgumentException("IP address cannot be null or empty.", nameof(ipAddress));

        Id = Guid.NewGuid();
        IpAddress = ipAddress;
        Reason = reason;
        CreatedAtUtc = DateTime.UtcNow;
        ExpiresAtUtc = expiresAtUtc;
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void UpdateReason(string? reason)
    {
        Reason = reason;
    }

    public bool IsExpired()
    {
        return ExpiresAtUtc.HasValue && ExpiresAtUtc.Value < DateTime.UtcNow;
    }
}

