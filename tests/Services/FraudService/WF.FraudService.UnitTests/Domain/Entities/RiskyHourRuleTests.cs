using FluentAssertions;
using WF.FraudService.Domain.Entities;
using WF.FraudService.Domain.ValueObjects;
using WF.Shared.Contracts.Result;
using Xunit;

namespace WF.FraudService.UnitTests.Domain.Entities;

public class RiskyHourRuleTests
{
    private readonly Bogus.Faker _faker = new();

    [Fact]
    public void Create_WithValidTimeRange_ShouldReturnSuccess()
    {
        // Arrange
        var timeRange = TimeRange.Create(22, 6).Value; // 22:00 - 06:00
        var description = _faker.Lorem.Sentence();

        // Act
        var result = RiskyHourRule.Create(timeRange, description);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.TimeRange.Should().Be(timeRange);
        result.Value.Description.Should().Be(description);
    }

    [Fact]
    public void Create_ShouldSetDefaultValues()
    {
        // Arrange
        var timeRange = TimeRange.Create(0, 23).Value;

        // Act
        var result = RiskyHourRule.Create(timeRange, null);
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
        var rule = CreateValidRiskyHourRule();

        // Act
        rule.Deactivate();

        // Assert
        rule.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_ShouldSetIsActiveTrue()
    {
        // Arrange
        var rule = CreateValidRiskyHourRule();
        rule.Deactivate();

        // Act
        rule.Activate();

        // Assert
        rule.IsActive.Should().BeTrue();
    }

    [Fact]
    public void UpdateHours_ShouldUpdate()
    {
        // Arrange
        var rule = CreateValidRiskyHourRule();
        var newTimeRange = TimeRange.Create(20, 4).Value;

        // Act
        rule.UpdateHours(newTimeRange);

        // Assert
        rule.TimeRange.Should().Be(newTimeRange);
    }

    [Fact]
    public void UpdateDescription_ShouldUpdate()
    {
        // Arrange
        var rule = CreateValidRiskyHourRule();
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
        var rule = CreateValidRiskyHourRule();
        var originalDescription = rule.Description;

        // Act
        rule.UpdateDescription(null);

        // Assert
        rule.Description.Should().BeNull();
    }

    private RiskyHourRule CreateValidRiskyHourRule()
    {
        var timeRange = TimeRange.Create(0, 23).Value;
        return RiskyHourRule.Create(timeRange, null).Value;
    }
}

