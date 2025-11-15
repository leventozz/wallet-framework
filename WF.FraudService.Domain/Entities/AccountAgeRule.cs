namespace WF.FraudService.Domain.Entities;

public class AccountAgeRule
{
    public Guid Id { get; private set; }
    public int MinAccountAgeDays { get; private set; }
    public decimal? MaxAllowedAmount { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private AccountAgeRule() { }

    public AccountAgeRule(int minAccountAgeDays, decimal? maxAllowedAmount = null, string? description = null)
    {
        if (minAccountAgeDays < 0)
            throw new ArgumentOutOfRangeException(nameof(minAccountAgeDays), "Minimum account age days cannot be negative.");

        if (maxAllowedAmount.HasValue && maxAllowedAmount.Value < 0)
            throw new ArgumentOutOfRangeException(nameof(maxAllowedAmount), "Maximum allowed amount cannot be negative.");

        Id = Guid.NewGuid();
        MinAccountAgeDays = minAccountAgeDays;
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

    public void UpdateMinAccountAgeDays(int minAccountAgeDays)
    {
        if (minAccountAgeDays < 0)
            throw new ArgumentOutOfRangeException(nameof(minAccountAgeDays), "Minimum account age days cannot be negative.");

        MinAccountAgeDays = minAccountAgeDays;
    }

    public void UpdateMaxAllowedAmount(decimal? maxAllowedAmount)
    {
        if (maxAllowedAmount.HasValue && maxAllowedAmount.Value < 0)
            throw new ArgumentOutOfRangeException(nameof(maxAllowedAmount), "Maximum allowed amount cannot be negative.");

        MaxAllowedAmount = maxAllowedAmount;
    }

    public bool IsAccountAgeSufficient(DateTime accountCreatedAtUtc)
    {
        var accountAgeDays = (DateTime.UtcNow - accountCreatedAtUtc).Days;
        return accountAgeDays >= MinAccountAgeDays;
    }

    public bool IsAmountAllowed(decimal amount, DateTime accountCreatedAtUtc)
    {
        if (!MaxAllowedAmount.HasValue)
            return true;

        if (!IsAccountAgeSufficient(accountCreatedAtUtc))
            return amount <= MaxAllowedAmount.Value;

        return true;
    }
}

