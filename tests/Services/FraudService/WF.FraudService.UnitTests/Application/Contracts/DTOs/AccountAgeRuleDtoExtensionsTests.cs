using FluentAssertions;
using WF.FraudService.Application.Contracts.DTOs;
using Xunit;

namespace WF.FraudService.UnitTests.Application.Contracts.DTOs;

public class AccountAgeRuleDtoExtensionsTests
{
    [Fact]
    public void IsAmountAllowed_WhenMaxAllowedAmountIsNull_ShouldReturnTrue()
    {
        // Arrange
        var dto = new AccountAgeRuleDto
        {
            Id = Guid.NewGuid(),
            MinAccountAgeDays = 30,
            MaxAllowedAmount = null,
            IsActive = true
        };
        var amount = 10000m;
        var accountAgeDays = 10;

        // Act
        var result = dto.IsAmountAllowed(amount, accountAgeDays);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAmountAllowed_WhenAccountAgeIsSufficient_ShouldReturnTrue()
    {
        // Arrange
        var dto = new AccountAgeRuleDto
        {
            Id = Guid.NewGuid(),
            MinAccountAgeDays = 30,
            MaxAllowedAmount = 1000m,
            IsActive = true
        };
        var amount = 5000m; // Exceeds max, but age is sufficient
        var accountAgeDays = 60; // More than 30 days

        // Act
        var result = dto.IsAmountAllowed(amount, accountAgeDays);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAmountAllowed_WhenAccountAgeIsInsufficient_AndAmountExceedsMax_ShouldReturnFalse()
    {
        // Arrange
        var dto = new AccountAgeRuleDto
        {
            Id = Guid.NewGuid(),
            MinAccountAgeDays = 30,
            MaxAllowedAmount = 1000m,
            IsActive = true
        };
        var amount = 2000m; // Exceeds max
        var accountAgeDays = 15; // Less than 30 days

        // Act
        var result = dto.IsAmountAllowed(amount, accountAgeDays);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsAmountAllowed_WhenAccountAgeIsInsufficient_AndAmountBelowMax_ShouldReturnTrue()
    {
        // Arrange
        var dto = new AccountAgeRuleDto
        {
            Id = Guid.NewGuid(),
            MinAccountAgeDays = 30,
            MaxAllowedAmount = 1000m,
            IsActive = true
        };
        var amount = 500m; // Below max
        var accountAgeDays = 15; // Less than 30 days

        // Act
        var result = dto.IsAmountAllowed(amount, accountAgeDays);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAmountAllowed_WhenAccountAgeEqualsMin_AndAmountExceedsMax_ShouldReturnFalse()
    {
        // Arrange
        var dto = new AccountAgeRuleDto
        {
            Id = Guid.NewGuid(),
            MinAccountAgeDays = 30,
            MaxAllowedAmount = 1000m,
            IsActive = true
        };
        var amount = 2000m; // Exceeds max
        var accountAgeDays = 30; // Exactly 30 days (still insufficient as it's < not <=)

        // Act
        var result = dto.IsAmountAllowed(amount, accountAgeDays);

        // Assert
        result.Should().BeFalse();
    }
}

