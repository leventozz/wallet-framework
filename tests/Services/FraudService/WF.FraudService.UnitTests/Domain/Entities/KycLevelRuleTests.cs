using FluentAssertions;
using WF.FraudService.Domain.Entities;
using WF.FraudService.Domain.ValueObjects;
using WF.Shared.Contracts.Enums;
using WF.Shared.Contracts.Result;
using Xunit;

namespace WF.FraudService.UnitTests.Domain.Entities;

public class KycLevelRuleTests
{
    private readonly Bogus.Faker _faker = new();

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var requiredKycStatus = KycStatus.EmailVerified;
        var maxAllowedAmount = Money.Create(_faker.Random.Decimal(100, 10000)).Value;
        var description = _faker.Lorem.Sentence();

        // Act
        var result = KycLevelRule.Create(requiredKycStatus, maxAllowedAmount, description);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.RequiredKycStatus.Should().Be(requiredKycStatus);
        result.Value.MaxAllowedAmount.Should().Be(maxAllowedAmount);
        result.Value.Description.Should().Be(description);
    }

    [Fact]
    public void Create_ShouldSetDefaultValues()
    {
        // Arrange
        var requiredKycStatus = KycStatus.Unverified;

        // Act
        var result = KycLevelRule.Create(requiredKycStatus, null, null);
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
        var rule = CreateValidKycLevelRule();

        // Act
        rule.Deactivate();

        // Assert
        rule.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_ShouldSetIsActiveTrue()
    {
        // Arrange
        var rule = CreateValidKycLevelRule();
        rule.Deactivate();

        // Act
        rule.Activate();

        // Assert
        rule.IsActive.Should().BeTrue();
    }

    [Fact]
    public void UpdateRequiredKycStatus_ShouldUpdate()
    {
        // Arrange
        var rule = CreateValidKycLevelRule();
        var newStatus = KycStatus.VideoVerified;

        // Act
        rule.UpdateRequiredKycStatus(newStatus);

        // Assert
        rule.RequiredKycStatus.Should().Be(newStatus);
    }

    [Fact]
    public void UpdateMaxAllowedAmount_ShouldUpdate()
    {
        // Arrange
        var rule = CreateValidKycLevelRule();
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
        var rule = CreateValidKycLevelRule();
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
        var rule = CreateValidKycLevelRule();
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
        var rule = CreateValidKycLevelRule();
        var originalDescription = rule.Description;

        // Act
        rule.UpdateDescription(null);

        // Assert
        rule.Description.Should().BeNull();
    }

    private KycLevelRule CreateValidKycLevelRule()
    {
        return KycLevelRule.Create(KycStatus.Unverified, null, null).Value;
    }
}

