using FluentAssertions;
using WF.FraudService.Domain.Entities;
using WF.FraudService.Domain.ValueObjects;
using WF.Shared.Contracts.Result;
using Xunit;

namespace WF.FraudService.UnitTests.Domain.Entities;

public class BlockedIpRuleTests
{
    private readonly Bogus.Faker _faker = new();

    [Fact]
    public void Create_WithValidIpAddress_ShouldReturnSuccess()
    {
        // Arrange
        var ipAddress = IpAddress.Create(_faker.Internet.Ip()).Value;
        var reason = _faker.Lorem.Sentence();
        var expiresAtUtc = DateTime.UtcNow.AddDays(7);

        // Act
        var result = BlockedIpRule.Create(ipAddress, reason, expiresAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.IpAddress.Should().Be(ipAddress);
        result.Value.Reason.Should().Be(reason);
        result.Value.ExpiresAtUtc.Should().Be(expiresAtUtc);
    }

    [Fact]
    public void Create_WithNullExpiresAtUtc_ShouldReturnSuccess()
    {
        // Arrange
        var ipAddress = IpAddress.Create(_faker.Internet.Ip()).Value;
        var reason = _faker.Lorem.Sentence();

        // Act
        var result = BlockedIpRule.Create(ipAddress, reason, null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ExpiresAtUtc.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldSetDefaultValues()
    {
        // Arrange
        var ipAddress = IpAddress.Create(_faker.Internet.Ip()).Value;

        // Act
        var result = BlockedIpRule.Create(ipAddress, null, null);
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
        var rule = CreateValidBlockedIpRule();

        // Act
        rule.Deactivate();

        // Assert
        rule.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_ShouldSetIsActiveTrue()
    {
        // Arrange
        var rule = CreateValidBlockedIpRule();
        rule.Deactivate();

        // Act
        rule.Activate();

        // Assert
        rule.IsActive.Should().BeTrue();
    }

    [Fact]
    public void UpdateReason_ShouldUpdate()
    {
        // Arrange
        var rule = CreateValidBlockedIpRule();
        var newReason = _faker.Lorem.Sentence();

        // Act
        rule.UpdateReason(newReason);

        // Assert
        rule.Reason.Should().Be(newReason);
    }

    [Fact]
    public void UpdateReason_WithNull_ShouldUpdate()
    {
        // Arrange
        var rule = CreateValidBlockedIpRule();
        var originalReason = rule.Reason;

        // Act
        rule.UpdateReason(null);

        // Assert
        rule.Reason.Should().BeNull();
    }

    private BlockedIpRule CreateValidBlockedIpRule()
    {
        var ipAddress = IpAddress.Create(_faker.Internet.Ip()).Value;
        return BlockedIpRule.Create(ipAddress, null, null).Value;
    }
}

