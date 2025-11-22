namespace WF.FraudService.Domain.ValueObjects;

public readonly record struct Money
{
    public decimal Amount { get; }

    public Money(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Money amount cannot be negative.");

        Amount = amount;
    }

    public static implicit operator decimal(Money money) => money.Amount;
    
    public static implicit operator Money(decimal amount) => new(amount);

    public static bool operator <(Money left, Money right) => left.Amount < right.Amount;
    
    public static bool operator >(Money left, Money right) => left.Amount > right.Amount;
    
    public static bool operator <=(Money left, Money right) => left.Amount <= right.Amount;
    
    public static bool operator >=(Money left, Money right) => left.Amount >= right.Amount;

    public override string ToString() => Amount.ToString("F2");
}

