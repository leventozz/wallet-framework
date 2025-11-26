using FluentAssertions;
using WF.Shared.Contracts.Result;
using WF.WalletService.Domain.ValueObjects;
using Xunit;

namespace WF.WalletService.UnitTests.Domain.ValueObjects;

public class MoneyTests
{
    private readonly Bogus.Faker _faker = new();

    [Fact]
    public void Create_WithValidAmount_ShouldReturnSuccess()
    {
        // Arrange
        var validAmount = _faker.Random.Decimal(1, 1000000);
        var currency = "TRY";

        // Act
        var result = Money.Create(validAmount, currency);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(validAmount);
        result.Value.Currency.Should().Be(currency);
    }

    [Fact]
    public void Create_WithZeroAmount_ShouldReturnSuccess()
    {
        // Arrange
        var zeroAmount = 0m;
        var currency = "TRY";

        // Act
        var result = Money.Create(zeroAmount, currency);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(0m);
        result.Value.Currency.Should().Be(currency);
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldReturnFailure()
    {
        // Arrange
        var negativeAmount = _faker.Random.Decimal(-1000, -1);
        var currency = "TRY";

        // Act
        var result = Money.Create(negativeAmount, currency);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Money.NegativeAmount");
        result.Error.Message.Should().Be("Money amount cannot be negative.");
    }

    [Fact]
    public void Create_WithNullCurrency_ShouldReturnFailure()
    {
        // Arrange
        var amount = _faker.Random.Decimal(1, 1000);
        string? currency = null;

        // Act
        var result = Money.Create(amount, currency!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Money.InvalidCurrency");
        result.Error.Message.Should().Be("Currency cannot be null or empty.");
    }

    [Fact]
    public void Create_WithEmptyCurrency_ShouldReturnFailure()
    {
        // Arrange
        var amount = _faker.Random.Decimal(1, 1000);
        var currency = string.Empty;

        // Act
        var result = Money.Create(amount, currency);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Money.InvalidCurrency");
        result.Error.Message.Should().Be("Currency cannot be null or empty.");
    }

    [Fact]
    public void Create_ShouldNormalizeCurrency_ToUpperCase()
    {
        // Arrange
        var amount = _faker.Random.Decimal(1, 1000);
        var currency = "try";

        // Act
        var result = Money.Create(amount, currency);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Currency.Should().Be("TRY");
    }

    [Fact]
    public void AdditionOperator_WithSameCurrency_ShouldWork()
    {
        // Arrange
        var money1 = Money.Create(100m, "TRY").Value;
        var money2 = Money.Create(50m, "TRY").Value;

        // Act
        var result = money1 + money2;

        // Assert
        result.Amount.Should().Be(150m);
        result.Currency.Should().Be("TRY");
    }

    [Fact]
    public void AdditionOperator_WithDifferentCurrency_ShouldThrowException()
    {
        // Arrange
        var money1 = Money.Create(100m, "TRY").Value;
        var money2 = Money.Create(50m, "USD").Value;

        // Act
        var act = () => money1 + money2;

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot operation money with different currencies: TRY and USD.");
    }

    [Fact]
    public void SubtractionOperator_WithSameCurrency_ShouldWork()
    {
        // Arrange
        var money1 = Money.Create(100m, "TRY").Value;
        var money2 = Money.Create(30m, "TRY").Value;

        // Act
        var result = money1 - money2;

        // Assert
        result.Amount.Should().Be(70m);
        result.Currency.Should().Be("TRY");
    }

    [Fact]
    public void SubtractionOperator_WithNegativeResult_ShouldThrowException()
    {
        // Arrange
        var money1 = Money.Create(50m, "TRY").Value;
        var money2 = Money.Create(100m, "TRY").Value;

        // Act
        var act = () => money1 - money2;

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Result of subtraction cannot be negative.");
    }

    [Fact]
    public void SubtractionOperator_WithDifferentCurrency_ShouldThrowException()
    {
        // Arrange
        var money1 = Money.Create(100m, "TRY").Value;
        var money2 = Money.Create(50m, "USD").Value;

        // Act
        var act = () => money1 - money2;

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot operation money with different currencies: TRY and USD.");
    }

    [Fact]
    public void ComparisonOperators_WithSameCurrency_ShouldWork()
    {
        // Arrange
        var amount1 = Money.Create(100m, "TRY").Value;
        var amount2 = Money.Create(200m, "TRY").Value;
        var amount3 = Money.Create(100m, "TRY").Value;

        // Act & Assert
        (amount1 < amount2).Should().BeTrue();
        (amount2 > amount1).Should().BeTrue();
        (amount1 <= amount3).Should().BeTrue();
        (amount1 >= amount3).Should().BeTrue();
        (amount1 <= amount2).Should().BeTrue();
        (amount2 >= amount1).Should().BeTrue();
    }

    [Fact]
    public void ComparisonOperators_WithDifferentCurrency_ShouldThrowException()
    {
        // Arrange
        var amount1 = Money.Create(100m, "TRY").Value;
        var amount2 = Money.Create(100m, "USD").Value;

        // Act & Assert
        var act1 = () => amount1 < amount2;
        act1.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot operation money with different currencies: TRY and USD.");

        var act2 = () => amount1 > amount2;
        act2.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot operation money with different currencies: TRY and USD.");
    }

    [Fact]
    public void ToString_ShouldReturnFormattedValue()
    {
        // Arrange
        var amount = 1234.56m;
        var money = Money.Create(amount, "TRY").Value;

        // Act
        var result = money.ToString();

        // Assert
        result.Should().Be("1234.56 TRY");
    }
}

