using WF.FraudService.Domain.ValueObjects;

namespace WF.FraudService.Domain.Entities;

public class BlockedIpRule
{
    public Guid Id { get; private set; }
    public IpAddress IpAddress { get; private set; }
    public string? Reason { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ExpiresAtUtc { get; private set; }
    public bool IsActive { get; private set; }

    private BlockedIpRule() { }

    public BlockedIpRule(IpAddress ipAddress, string? reason = null, DateTime? expiresAtUtc = null)
    {
        Id = Guid.NewGuid();
        IpAddress = ipAddress;
        Reason = reason;
        CreatedAtUtc = DateTime.UtcNow;
        ExpiresAtUtc = expiresAtUtc;
        IsActive = true;
    }

    public BlockedIpRule(
        Guid id,
        IpAddress ipAddress,
        string? reason,
        DateTime createdAtUtc,
        DateTime? expiresAtUtc,
        bool isActive)
    {
        Id = id;
        IpAddress = ipAddress;
        Reason = reason;
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
        IsActive = isActive;
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
}

