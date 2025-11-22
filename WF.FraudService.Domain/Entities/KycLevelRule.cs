using WF.FraudService.Domain.ValueObjects;
using WF.Shared.Contracts.Enums;

namespace WF.FraudService.Domain.Entities;

public class KycLevelRule
{
    public Guid Id { get; private set; }
    public KycStatus RequiredKycStatus { get; private set; }
    public Money? MaxAllowedAmount { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private KycLevelRule() { }

    public KycLevelRule(KycStatus requiredKycStatus, Money? maxAllowedAmount = null, string? description = null)
    {
        Id = Guid.NewGuid();
        RequiredKycStatus = requiredKycStatus;
        MaxAllowedAmount = maxAllowedAmount;
        Description = description;
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public KycLevelRule(
        Guid id,
        KycStatus requiredKycStatus,
        Money? maxAllowedAmount,
        string? description,
        DateTime createdAtUtc,
        bool isActive)
    {
        Id = id;
        RequiredKycStatus = requiredKycStatus;
        MaxAllowedAmount = maxAllowedAmount;
        Description = description;
        CreatedAtUtc = createdAtUtc;
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

    public void UpdateDescription(string? description)
    {
        Description = description;
    }

    public void UpdateRequiredKycStatus(KycStatus requiredKycStatus)
    {
        RequiredKycStatus = requiredKycStatus;
    }

    public void UpdateMaxAllowedAmount(Money? maxAllowedAmount)
    {
        MaxAllowedAmount = maxAllowedAmount;
    }
}

