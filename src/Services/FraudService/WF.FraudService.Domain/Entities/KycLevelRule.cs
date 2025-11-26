using WF.FraudService.Domain.ValueObjects;
using WF.Shared.Contracts.Enums;
using WF.Shared.Contracts.Result;

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

    public static Result<KycLevelRule> Create(KycStatus requiredKycStatus, Money? maxAllowedAmount, string? description)
    {
        return Result<KycLevelRule>.Success(new KycLevelRule
        {
            Id = Guid.NewGuid(),
            RequiredKycStatus = requiredKycStatus,
            MaxAllowedAmount = maxAllowedAmount,
            Description = description,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        });
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

