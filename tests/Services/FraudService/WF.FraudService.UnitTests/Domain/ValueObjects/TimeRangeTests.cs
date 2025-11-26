using FluentAssertions;
using WF.FraudService.Domain.ValueObjects;
using WF.Shared.Contracts.Result;
using Xunit;

namespace WF.FraudService.UnitTests.Domain.ValueObjects;

public class TimeRangeTests
{
    [Fact]
    public void Create_WithValidHours_ShouldReturnSuccess()
    {
        // Arrange
        var startHour = 9;
        var endHour = 17;

        // Act
        var result = TimeRange.Create(startHour, endHour);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.StartHour.Should().Be(startHour);
        result.Value.EndHour.Should().Be(endHour);
    }

    [Fact]
    public void Create_WithStartHourLessThanZero_ShouldReturnFailure()
    {
        // Arrange
        var startHour = -1;
        var endHour = 17;

        // Act
        var result = TimeRange.Create(startHour, endHour);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TimeRange.InvalidHour");
        result.Error.Message.Should().Contain("Start hour must be between 0 and 23");
    }

    [Fact]
    public void Create_WithStartHourGreaterThan23_ShouldReturnFailure()
    {
        // Arrange
        var startHour = 24;
        var endHour = 17;

        // Act
        var result = TimeRange.Create(startHour, endHour);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TimeRange.InvalidHour");
    }

    [Fact]
    public void Create_WithEndHourLessThanZero_ShouldReturnFailure()
    {
        // Arrange
        var startHour = 9;
        var endHour = -1;

        // Act
        var result = TimeRange.Create(startHour, endHour);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TimeRange.InvalidHour");
        result.Error.Message.Should().Contain("End hour must be between 0 and 23");
    }

    [Fact]
    public void Create_WithEndHourGreaterThan23_ShouldReturnFailure()
    {
        // Arrange
        var startHour = 9;
        var endHour = 24;

        // Act
        var result = TimeRange.Create(startHour, endHour);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TimeRange.InvalidHour");
    }

    [Fact]
    public void FromDatabaseValue_WithValidValues_ShouldReturn()
    {
        // Arrange
        var startHour = 9;
        var endHour = 17;

        // Act
        var timeRange = TimeRange.FromDatabaseValue(startHour, endHour);

        // Assert
        timeRange.StartHour.Should().Be(startHour);
        timeRange.EndHour.Should().Be(endHour);
    }

    [Fact]
    public void FromDatabaseValue_WithInvalidStartHour_ShouldThrow()
    {
        // Arrange
        var startHour = -1;
        var endHour = 17;

        // Act & Assert
        var act = () => TimeRange.FromDatabaseValue(startHour, endHour);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Start hour must be between 0 and 23 when reading from database.");
    }

    [Fact]
    public void FromDatabaseValue_WithInvalidEndHour_ShouldThrow()
    {
        // Arrange
        var startHour = 9;
        var endHour = 24;

        // Act & Assert
        var act = () => TimeRange.FromDatabaseValue(startHour, endHour);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("End hour must be between 0 and 23 when reading from database.");
    }

    [Fact]
    public void IsCurrentTimeInRange_WhenStartLessThanEnd_AndTimeInRange_ShouldReturnTrue()
    {
        // Arrange
        var timeRange = TimeRange.Create(9, 17).Value;
        var dateTime = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc); // 12:00

        // Act
        var result = timeRange.IsCurrentTimeInRange(dateTime);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsCurrentTimeInRange_WhenStartLessThanEnd_AndTimeOutOfRange_ShouldReturnFalse()
    {
        // Arrange
        var timeRange = TimeRange.Create(9, 17).Value;
        var dateTime = new DateTime(2024, 1, 1, 20, 0, 0, DateTimeKind.Utc); // 20:00

        // Act
        var result = timeRange.IsCurrentTimeInRange(dateTime);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsCurrentTimeInRange_WhenStartLessThanEnd_AndTimeAtStart_ShouldReturnTrue()
    {
        // Arrange
        var timeRange = TimeRange.Create(9, 17).Value;
        var dateTime = new DateTime(2024, 1, 1, 9, 0, 0, DateTimeKind.Utc); // 09:00

        // Act
        var result = timeRange.IsCurrentTimeInRange(dateTime);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsCurrentTimeInRange_WhenStartLessThanEnd_AndTimeAtEnd_ShouldReturnTrue()
    {
        // Arrange
        var timeRange = TimeRange.Create(9, 17).Value;
        var dateTime = new DateTime(2024, 1, 1, 17, 0, 0, DateTimeKind.Utc); // 17:00

        // Act
        var result = timeRange.IsCurrentTimeInRange(dateTime);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsCurrentTimeInRange_WhenStartGreaterThanEnd_AndTimeInRange_ShouldReturnTrue()
    {
        // Arrange - Gece yarısı geçişi: 22:00 - 06:00
        var timeRange = TimeRange.Create(22, 6).Value;
        var dateTime = new DateTime(2024, 1, 1, 2, 0, 0, DateTimeKind.Utc); // 02:00 (gece yarısından sonra)

        // Act
        var result = timeRange.IsCurrentTimeInRange(dateTime);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsCurrentTimeInRange_WhenStartGreaterThanEnd_AndTimeInRangeBeforeMidnight_ShouldReturnTrue()
    {
        // Arrange - Gece yarısı geçişi: 22:00 - 06:00
        var timeRange = TimeRange.Create(22, 6).Value;
        var dateTime = new DateTime(2024, 1, 1, 23, 0, 0, DateTimeKind.Utc); // 23:00 (gece yarısından önce)

        // Act
        var result = timeRange.IsCurrentTimeInRange(dateTime);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsCurrentTimeInRange_WhenStartGreaterThanEnd_AndTimeOutOfRange_ShouldReturnFalse()
    {
        // Arrange - Gece yarısı geçişi: 22:00 - 06:00
        var timeRange = TimeRange.Create(22, 6).Value;
        var dateTime = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc); // 12:00 (öğlen)

        // Act
        var result = timeRange.IsCurrentTimeInRange(dateTime);

        // Assert
        result.Should().BeFalse();
    }
}

