using WF.FraudService.Domain.ValueObjects;

namespace WF.FraudService.Domain.Entities;

public class AccountAgeRule
{
    public Guid Id { get; private set; }
    public int MinAccountAgeDays { get; private set; }
    public Money? MaxAllowedAmount { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private AccountAgeRule() { }

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

    public void UpdateMinAccountAgeDays(int minAccountAgeDays)
    {
        if (minAccountAgeDays < 0)
            throw new ArgumentOutOfRangeException(nameof(minAccountAgeDays), "Minimum account age days cannot be negative.");

        MinAccountAgeDays = minAccountAgeDays;
    }

    public void UpdateMaxAllowedAmount(Money? maxAllowedAmount)
    {
        MaxAllowedAmount = maxAllowedAmount;
    }
}

