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

