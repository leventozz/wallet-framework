using FluentAssertions;
using WF.FraudService.Application.Contracts.DTOs;
using Xunit;

namespace WF.FraudService.UnitTests.Application.Contracts.DTOs;

public class BlockedIpRuleDtoExtensionsTests
{
    [Fact]
    public void IsExpired_WhenExpiresAtUtcIsNull_ShouldReturnFalse()
    {
        // Arrange
        var dto = new BlockedIpRuleDto
        {
            Id = Guid.NewGuid(),
            IpAddress = "192.168.1.1",
            ExpiresAtUtc = null,
            IsActive = true
        };
        var utcNow = DateTime.UtcNow;

        // Act
        var result = dto.IsExpired(utcNow);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WhenExpiresAtUtcIsFuture_ShouldReturnFalse()
    {
        // Arrange
        var dto = new BlockedIpRuleDto
        {
            Id = Guid.NewGuid(),
            IpAddress = "192.168.1.1",
            ExpiresAtUtc = DateTime.UtcNow.AddDays(7),
            IsActive = true
        };
        var utcNow = DateTime.UtcNow;

        // Act
        var result = dto.IsExpired(utcNow);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WhenExpiresAtUtcIsPast_ShouldReturnTrue()
    {
        // Arrange
        var dto = new BlockedIpRuleDto
        {
            Id = Guid.NewGuid(),
            IpAddress = "192.168.1.1",
            ExpiresAtUtc = DateTime.UtcNow.AddDays(-1),
            IsActive = true
        };
        var utcNow = DateTime.UtcNow;

        // Act
        var result = dto.IsExpired(utcNow);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_WhenExpiresAtUtcEqualsNow_ShouldReturnTrue()
    {
        // Arrange
        var utcNow = DateTime.UtcNow;
        var dto = new BlockedIpRuleDto
        {
            Id = Guid.NewGuid(),
            IpAddress = "192.168.1.1",
            ExpiresAtUtc = utcNow,
            IsActive = true
        };

        // Act
        var result = dto.IsExpired(utcNow);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsBlocked_WhenDtoIsNull_ShouldReturnFalse()
    {
        // Arrange
        BlockedIpRuleDto? dto = null;
        var utcNow = DateTime.UtcNow;

        // Act
        var result = dto.IsBlocked(utcNow);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsBlocked_WhenIsActiveFalse_ShouldReturnFalse()
    {
        // Arrange
        var dto = new BlockedIpRuleDto
        {
            Id = Guid.NewGuid(),
            IpAddress = "192.168.1.1",
            ExpiresAtUtc = DateTime.UtcNow.AddDays(7),
            IsActive = false
        };
        var utcNow = DateTime.UtcNow;

        // Act
        var result = dto.IsBlocked(utcNow);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsBlocked_WhenExpired_ShouldReturnFalse()
    {
        // Arrange
        var dto = new BlockedIpRuleDto
        {
            Id = Guid.NewGuid(),
            IpAddress = "192.168.1.1",
            ExpiresAtUtc = DateTime.UtcNow.AddDays(-1),
            IsActive = true
        };
        var utcNow = DateTime.UtcNow;

        // Act
        var result = dto.IsBlocked(utcNow);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsBlocked_WhenActiveAndNotExpired_ShouldReturnTrue()
    {
        // Arrange
        var dto = new BlockedIpRuleDto
        {
            Id = Guid.NewGuid(),
            IpAddress = "192.168.1.1",
            ExpiresAtUtc = DateTime.UtcNow.AddDays(7),
            IsActive = true
        };
        var utcNow = DateTime.UtcNow;

        // Act
        var result = dto.IsBlocked(utcNow);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsBlocked_WhenActiveAndNoExpiration_ShouldReturnTrue()
    {
        // Arrange
        var dto = new BlockedIpRuleDto
        {
            Id = Guid.NewGuid(),
            IpAddress = "192.168.1.1",
            ExpiresAtUtc = null,
            IsActive = true
        };
        var utcNow = DateTime.UtcNow;

        // Act
        var result = dto.IsBlocked(utcNow);

        // Assert
        result.Should().BeTrue();
    }
}

