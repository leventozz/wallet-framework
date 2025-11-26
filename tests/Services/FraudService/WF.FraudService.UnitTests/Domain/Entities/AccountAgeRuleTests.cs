using FluentAssertions;
using WF.FraudService.Domain.Entities;
using WF.FraudService.Domain.ValueObjects;
using WF.Shared.Contracts.Result;
using Xunit;

namespace WF.FraudService.UnitTests.Domain.Entities;

public class AccountAgeRuleTests
{
    private readonly Bogus.Faker _faker = new();

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var minAccountAgeDays = _faker.Random.Int(1, 365);
        var maxAllowedAmount = Money.Create(_faker.Random.Decimal(100, 10000)).Value;
        var description = _faker.Lorem.Sentence();

        // Act
        var result = AccountAgeRule.Create(minAccountAgeDays, maxAllowedAmount, description);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.MinAccountAgeDays.Should().Be(minAccountAgeDays);
        result.Value.MaxAllowedAmount.Should().Be(maxAllowedAmount);
        result.Value.Description.Should().Be(description);
    }

    [Fact]
    public void Create_WithNegativeMinAccountAgeDays_ShouldReturnFailure()
    {
        // Arrange
        var negativeDays = _faker.Random.Int(-100, -1);

        // Act
        var result = AccountAgeRule.Create(negativeDays, null, null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AccountAgeRule.NegativeMinAccountAgeDays");
        result.Error.Message.Should().Be("Minimum account age days cannot be negative.");
    }

    [Fact]
    public void Create_ShouldSetDefaultValues()
    {
        // Arrange
        var minAccountAgeDays = _faker.Random.Int(0, 365);

        // Act
        var result = AccountAgeRule.Create(minAccountAgeDays, null, null);
        var rule = result.Value;

        // Assert
        rule.Id.Should().NotBeEmpty();
        rule.IsActive.Should().BeTrue();
        rule.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        // Arrange
        var rule = CreateValidAccountAgeRule();

        // Act
        rule.Deactivate();

        // Assert
        rule.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_ShouldSetIsActiveTrue()
    {
        // Arrange
        var rule = CreateValidAccountAgeRule();
        rule.Deactivate();

        // Act
        rule.Activate();

        // Assert
        rule.IsActive.Should().BeTrue();
    }

    [Fact]
    public void UpdateMinAccountAgeDays_WithValidValue_ShouldUpdate()
    {
        // Arrange
        var rule = CreateValidAccountAgeRule();
        var newMinDays = _faker.Random.Int(1, 365);

        // Act
        var result = rule.UpdateMinAccountAgeDays(newMinDays);

        // Assert
        result.IsSuccess.Should().BeTrue();
        rule.MinAccountAgeDays.Should().Be(newMinDays);
    }

    [Fact]
    public void UpdateMinAccountAgeDays_WithNegativeValue_ShouldReturnFailure()
    {
        // Arrange
        var rule = CreateValidAccountAgeRule();
        var negativeDays = _faker.Random.Int(-100, -1);
        var originalDays = rule.MinAccountAgeDays;

        // Act
        var result = rule.UpdateMinAccountAgeDays(negativeDays);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AccountAgeRule.NegativeMinAccountAgeDays");
        rule.MinAccountAgeDays.Should().Be(originalDays);
    }

    [Fact]
    public void UpdateMaxAllowedAmount_ShouldUpdate()
    {
        // Arrange
        var rule = CreateValidAccountAgeRule();
        var newAmount = Money.Create(_faker.Random.Decimal(100, 10000)).Value;

        // Act
        rule.UpdateMaxAllowedAmount(newAmount);

        // Assert
        rule.MaxAllowedAmount.Should().Be(newAmount);
    }

    [Fact]
    public void UpdateMaxAllowedAmount_WithNull_ShouldUpdate()
    {
        // Arrange
        var rule = CreateValidAccountAgeRule();
        var originalAmount = rule.MaxAllowedAmount;

        // Act
        rule.UpdateMaxAllowedAmount(null);

        // Assert
        rule.MaxAllowedAmount.Should().BeNull();
    }

    [Fact]
    public void UpdateDescription_ShouldUpdate()
    {
        // Arrange
        var rule = CreateValidAccountAgeRule();
        var newDescription = _faker.Lorem.Sentence();

        // Act
        rule.UpdateDescription(newDescription);

        // Assert
        rule.Description.Should().Be(newDescription);
    }

    [Fact]
    public void UpdateDescription_WithNull_ShouldUpdate()
    {
        // Arrange
        var rule = CreateValidAccountAgeRule();
        var originalDescription = rule.Description;

        // Act
        rule.UpdateDescription(null);

        // Assert
        rule.Description.Should().BeNull();
    }

    private AccountAgeRule CreateValidAccountAgeRule()
    {
        var minAccountAgeDays = _faker.Random.Int(0, 365);
        return AccountAgeRule.Create(minAccountAgeDays, null, null).Value;
    }
}

