using FluentAssertions;
using WF.FraudService.Application.Contracts.DTOs;
using WF.FraudService.Domain.ValueObjects;
using WF.Shared.Contracts.Result;
using Xunit;

namespace WF.FraudService.UnitTests.Application.Contracts.DTOs;

public class RiskyHourRuleDtoExtensionsTests
{
    [Fact]
    public void ToTimeRange_WithValidHours_ShouldReturnSuccess()
    {
        // Arrange
        var dto = new RiskyHourRuleDto
        {
            Id = Guid.NewGuid(),
            StartHour = 22,
            EndHour = 6,
            IsActive = true
        };

        // Act
        var result = dto.ToTimeRange();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.StartHour.Should().Be(22);
        result.Value.EndHour.Should().Be(6);
    }

    [Fact]
    public void ToTimeRange_WithInvalidHours_ShouldReturnFailure()
    {
        // Arrange
        var dto = new RiskyHourRuleDto
        {
            Id = Guid.NewGuid(),
            StartHour = 25, // Invalid
            EndHour = 6,
            IsActive = true
        };

        // Act
        var result = dto.ToTimeRange();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TimeRange.InvalidHour");
    }

    [Fact]
    public void IsCurrentTimeRisky_WhenTimeInRange_ShouldReturnTrue()
    {
        // Arrange
        var dto = new RiskyHourRuleDto
        {
            Id = Guid.NewGuid(),
            StartHour = 9,
            EndHour = 17,
            IsActive = true
        };
        var utcDateTime = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc); // 12:00 (in range)

        // Act
        var result = dto.IsCurrentTimeRisky(utcDateTime);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public void IsCurrentTimeRisky_WhenTimeOutOfRange_ShouldReturnFalse()
    {
        // Arrange
        var dto = new RiskyHourRuleDto
        {
            Id = Guid.NewGuid(),
            StartHour = 9,
            EndHour = 17,
            IsActive = true
        };
        var utcDateTime = new DateTime(2024, 1, 1, 20, 0, 0, DateTimeKind.Utc); // 20:00 (out of range)

        // Act
        var result = dto.IsCurrentTimeRisky(utcDateTime);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public void IsCurrentTimeRisky_WhenTimeAtStart_ShouldReturnTrue()
    {
        // Arrange
        var dto = new RiskyHourRuleDto
        {
            Id = Guid.NewGuid(),
            StartHour = 9,
            EndHour = 17,
            IsActive = true
        };
        var utcDateTime = new DateTime(2024, 1, 1, 9, 0, 0, DateTimeKind.Utc); // 09:00 (at start)

        // Act
        var result = dto.IsCurrentTimeRisky(utcDateTime);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public void IsCurrentTimeRisky_WhenTimeAtEnd_ShouldReturnTrue()
    {
        // Arrange
        var dto = new RiskyHourRuleDto
        {
            Id = Guid.NewGuid(),
            StartHour = 9,
            EndHour = 17,
            IsActive = true
        };
        var utcDateTime = new DateTime(2024, 1, 1, 17, 0, 0, DateTimeKind.Utc); // 17:00 (at end)

        // Act
        var result = dto.IsCurrentTimeRisky(utcDateTime);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public void IsCurrentTimeRisky_WhenTimeRangeCrossesMidnight_AndTimeInRange_ShouldReturnTrue()
    {
        // Arrange
        var dto = new RiskyHourRuleDto
        {
            Id = Guid.NewGuid(),
            StartHour = 22,
            EndHour = 6,
            IsActive = true
        };
        var utcDateTime = new DateTime(2024, 1, 1, 2, 0, 0, DateTimeKind.Utc); // 02:00 (after midnight, in range)

        // Act
        var result = dto.IsCurrentTimeRisky(utcDateTime);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public void IsCurrentTimeRisky_WhenTimeRangeCrossesMidnight_AndTimeOutOfRange_ShouldReturnFalse()
    {
        // Arrange
        var dto = new RiskyHourRuleDto
        {
            Id = Guid.NewGuid(),
            StartHour = 22,
            EndHour = 6,
            IsActive = true
        };
        var utcDateTime = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc); // 12:00 (out of range)

        // Act
        var result = dto.IsCurrentTimeRisky(utcDateTime);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }
}

