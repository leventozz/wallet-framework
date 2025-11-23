using WF.Shared.Contracts.Result;

namespace WF.WalletService.Domain.ValueObjects;

public readonly record struct Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Result<Money> Create(decimal amount, string currency)
    {
        if (amount < 0)
            return Result<Money>.Failure(Error.Validation("Money.NegativeAmount", "Money amount cannot be negative."));

        if (string.IsNullOrWhiteSpace(currency))
            return Result<Money>.Failure(Error.Validation("Money.InvalidCurrency", "Currency cannot be null or empty."));

        return Result<Money>.Success(new Money(amount, currency.Trim().ToUpperInvariant()));
    }

    private static void ValidateSameCurrency(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException($"Cannot operation money with different currencies: {left.Currency} and {right.Currency}.");
    }

    public static Money operator +(Money left, Money right)
    {
        ValidateSameCurrency(left, right);
        return new Money(left.Amount + right.Amount, left.Currency);
    }

    public static Money operator -(Money left, Money right)
    {
        ValidateSameCurrency(left, right);
        var result = left.Amount - right.Amount;
        if (result < 0)
            throw new InvalidOperationException("Result of subtraction cannot be negative.");

        return new Money(result, left.Currency);
    }

    public static bool operator <(Money left, Money right)
    {
        ValidateSameCurrency(left, right);
        return left.Amount < right.Amount;
    }

    public static bool operator >(Money left, Money right)
    {
        ValidateSameCurrency(left, right);
        return left.Amount > right.Amount;
    }

    public static bool operator <=(Money left, Money right)
    {
        ValidateSameCurrency(left, right);
        return left.Amount <= right.Amount;
    }

    public static bool operator >=(Money left, Money right)
    {
        ValidateSameCurrency(left, right);
        return left.Amount >= right.Amount;
    }

    public override string ToString() => $"{Amount:F2} {Currency}";
}

