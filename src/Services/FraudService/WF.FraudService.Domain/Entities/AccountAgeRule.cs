using WF.FraudService.Domain.ValueObjects;
using WF.Shared.Contracts.Result;

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

    public static Result<AccountAgeRule> Create(int minAccountAgeDays, Money? maxAllowedAmount, string? description)
    {
        if (minAccountAgeDays < 0)
            return Result<AccountAgeRule>.Failure(Error.Validation("AccountAgeRule.NegativeMinAccountAgeDays", "Minimum account age days cannot be negative."));

        return Result<AccountAgeRule>.Success(new AccountAgeRule
        {
            Id = Guid.NewGuid(),
            MinAccountAgeDays = minAccountAgeDays,
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

    public Result UpdateMinAccountAgeDays(int minAccountAgeDays)
    {
        if (minAccountAgeDays < 0)
            return Result.Failure(Error.Validation("AccountAgeRule.NegativeMinAccountAgeDays", "Minimum account age days cannot be negative."));

        MinAccountAgeDays = minAccountAgeDays;
        return Result.Success();
    }

    public void UpdateMaxAllowedAmount(Money? maxAllowedAmount)
    {
        MaxAllowedAmount = maxAllowedAmount;
    }
}

