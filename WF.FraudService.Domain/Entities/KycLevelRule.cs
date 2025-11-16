using WF.Shared.Contracts.Enums;

namespace WF.FraudService.Domain.Entities;

public class KycLevelRule
{
    public Guid Id { get; private set; }
    public KycStatus RequiredKycStatus { get; private set; }
    public decimal? MaxAllowedAmount { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private KycLevelRule() { }

    public KycLevelRule(KycStatus requiredKycStatus, decimal? maxAllowedAmount = null, string? description = null)
    {
        if (maxAllowedAmount.HasValue && maxAllowedAmount.Value < 0)
            throw new ArgumentOutOfRangeException(nameof(maxAllowedAmount), "Maximum allowed amount cannot be negative.");

        Id = Guid.NewGuid();
        RequiredKycStatus = requiredKycStatus;
        MaxAllowedAmount = maxAllowedAmount;
        Description = description;
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
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

    public void UpdateMaxAllowedAmount(decimal? maxAllowedAmount)
    {
        if (maxAllowedAmount.HasValue && maxAllowedAmount.Value < 0)
            throw new ArgumentOutOfRangeException(nameof(maxAllowedAmount), "Maximum allowed amount cannot be negative.");

        MaxAllowedAmount = maxAllowedAmount;
    }

    public bool IsKycStatusSufficient(KycStatus customerKycStatus)
    {
        return customerKycStatus >= RequiredKycStatus;
    }

    public bool IsAmountAllowed(decimal amount, KycStatus customerKycStatus)
    {
        if (!MaxAllowedAmount.HasValue)
            return true;

        if (!IsKycStatusSufficient(customerKycStatus))
            return amount <= MaxAllowedAmount.Value;

        return true;
    }
}

