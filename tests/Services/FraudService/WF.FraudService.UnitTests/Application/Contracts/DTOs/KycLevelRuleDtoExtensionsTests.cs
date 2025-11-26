using FluentAssertions;
using WF.FraudService.Application.Contracts.DTOs;
using WF.Shared.Contracts.Enums;
using WF.Shared.Contracts.Result;
using Xunit;

namespace WF.FraudService.UnitTests.Application.Contracts.DTOs;

public class KycLevelRuleDtoExtensionsTests
{
    [Fact]
    public void IsAmountAllowed_WhenMaxAllowedAmountIsNull_ShouldReturnTrue()
    {
        // Arrange
        var dto = new KycLevelRuleDto
        {
            Id = Guid.NewGuid(),
            RequiredKycStatus = KycStatus.EmailVerified,
            MaxAllowedAmount = null,
            IsActive = true
        };
        var amount = 10000m;
        var customerKycStatus = KycStatus.Unverified;

        // Act
        var result = dto.IsAmountAllowed(amount, customerKycStatus);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAmountAllowed_WhenCustomerKycStatusIsHigher_ShouldReturnTrue()
    {
        // Arrange
        var dto = new KycLevelRuleDto
        {
            Id = Guid.NewGuid(),
            RequiredKycStatus = KycStatus.EmailVerified,
            MaxAllowedAmount = 1000m,
            IsActive = true
        };
        var amount = 5000m; // Exceeds max, but KYC level is higher
        var customerKycStatus = KycStatus.VideoVerified; // Higher than required

        // Act
        var result = dto.IsAmountAllowed(amount, customerKycStatus);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAmountAllowed_WhenAmountBelowMax_ShouldReturnTrue()
    {
        // Arrange
        var dto = new KycLevelRuleDto
        {
            Id = Guid.NewGuid(),
            RequiredKycStatus = KycStatus.EmailVerified,
            MaxAllowedAmount = 1000m,
            IsActive = true
        };
        var amount = 500m; // Below max
        var customerKycStatus = KycStatus.Unverified; // Lower than required

        // Act
        var result = dto.IsAmountAllowed(amount, customerKycStatus);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAmountAllowed_WhenAmountExceedsMax_ShouldReturnFalse()
    {
        // Arrange
        var dto = new KycLevelRuleDto
        {
            Id = Guid.NewGuid(),
            RequiredKycStatus = KycStatus.EmailVerified,
            MaxAllowedAmount = 1000m,
            IsActive = true
        };
        var amount = 2000m; // Exceeds max
        var customerKycStatus = KycStatus.Unverified; // Lower than required

        // Act
        var result = dto.IsAmountAllowed(amount, customerKycStatus);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsAmountAllowed_WhenCustomerKycStatusEqualsRequired_AndAmountExceedsMax_ShouldReturnFalse()
    {
        // Arrange
        var dto = new KycLevelRuleDto
        {
            Id = Guid.NewGuid(),
            RequiredKycStatus = KycStatus.EmailVerified,
            MaxAllowedAmount = 1000m,
            IsActive = true
        };
        var amount = 2000m; // Exceeds max
        var customerKycStatus = KycStatus.EmailVerified; // Equals required (not higher)

        // Act
        var result = dto.IsAmountAllowed(amount, customerKycStatus);

        // Assert
        result.Should().BeFalse();
    }
}

