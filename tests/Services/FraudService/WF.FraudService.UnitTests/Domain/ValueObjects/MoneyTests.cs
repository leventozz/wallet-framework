using FluentAssertions;
using WF.FraudService.Domain.ValueObjects;

namespace WF.FraudService.UnitTests.Domain.ValueObjects;

public class MoneyTests
{
    private readonly Bogus.Faker _faker = new();

    [Fact]
    public void Create_WithValidAmount_ShouldReturnSuccess()
    {
        // Arrange
        var validAmount = _faker.Random.Decimal(1, 1000000);

        // Act
        var result = Money.Create(validAmount);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(validAmount);
    }

    [Fact]
    public void Create_WithZeroAmount_ShouldReturnSuccess()
    {
        // Arrange
        var zeroAmount = 0m;

        // Act
        var result = Money.Create(zeroAmount);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(0m);
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldReturnFailure()
    {
        // Arrange
        var negativeAmount = _faker.Random.Decimal(-1000, -1);

        // Act
        var result = Money.Create(negativeAmount);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Money.NegativeAmount");
        result.Error.Message.Should().Be("Money amount cannot be negative.");
    }

    [Fact]
    public void FromDatabaseValue_WithValidValue_ShouldReturn()
    {
        // Arrange
        var validAmount = _faker.Random.Decimal(1, 1000000);

        // Act
        var money = Money.FromDatabaseValue(validAmount);

        // Assert
        money.Amount.Should().Be(validAmount);
    }

    [Fact]
    public void FromDatabaseValue_WithNullValue_ShouldThrow()
    {
        // Arrange
        decimal? nullValue = null;

        // Act & Assert
        var act = () => Money.FromDatabaseValue(nullValue);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Money amount cannot be null when reading from database.");
    }

    [Fact]
    public void FromDatabaseValue_WithNegativeValue_ShouldThrow()
    {
        // Arrange
        var negativeValue = _faker.Random.Decimal(-1000, -1);

        // Act & Assert
        var act = () => Money.FromDatabaseValue(negativeValue);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Money amount cannot be negative when reading from database.");
    }

    [Fact]
    public void ImplicitOperator_Decimal_ShouldReturnAmount()
    {
        // Arrange
        var amount = _faker.Random.Decimal(1, 1000000);
        var money = Money.Create(amount).Value;

        // Act
        decimal result = money;

        // Assert
        result.Should().Be(amount);
    }

    [Fact]
    public void ComparisonOperators_ShouldWorkCorrectly()
    {
        // Arrange
        var amount1 = Money.Create(100m).Value;
        var amount2 = Money.Create(200m).Value;
        var amount3 = Money.Create(100m).Value;

        // Act & Assert
        (amount1 < amount2).Should().BeTrue();
        (amount2 > amount1).Should().BeTrue();
        (amount1 <= amount3).Should().BeTrue();
        (amount1 >= amount3).Should().BeTrue();
        (amount1 <= amount2).Should().BeTrue();
        (amount2 >= amount1).Should().BeTrue();
    }

    [Fact]
    public void ToString_ShouldReturnFormattedValue()
    {
        // Arrange
        var amount = 1234.56m;
        var money = Money.Create(amount).Value;

        // Act
        var result = money.ToString();

        // Assert
        result.Should().Be("1234.56");
    }
}

