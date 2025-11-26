using System.Globalization;
using WF.Shared.Contracts.Result;

namespace WF.FraudService.Domain.ValueObjects;

public readonly record struct Money
{
    public decimal Amount { get; }

    private Money(decimal amount)
    {
        Amount = amount;
    }

    public static Result<Money> Create(decimal amount)
    {
        if (amount < 0)
            return Result<Money>.Failure(Error.Validation("Money.NegativeAmount", "Money amount cannot be negative."));

        return Result<Money>.Success(new Money(amount));
    }

    // for efcore
    public static Money FromDatabaseValue(decimal? value)
    {
        if (!value.HasValue)
            throw new InvalidOperationException("Money amount cannot be null when reading from database.");

        if (value.Value < 0)
            throw new InvalidOperationException("Money amount cannot be negative when reading from database.");

        return new Money(value.Value);
    }

    public static implicit operator decimal(Money money) => money.Amount;

    public static bool operator <(Money left, Money right) => left.Amount < right.Amount;
    
    public static bool operator >(Money left, Money right) => left.Amount > right.Amount;
    
    public static bool operator <=(Money left, Money right) => left.Amount <= right.Amount;
    
    public static bool operator >=(Money left, Money right) => left.Amount >= right.Amount;

    public override string ToString() => Amount.ToString("F2", CultureInfo.InvariantCulture);
}

