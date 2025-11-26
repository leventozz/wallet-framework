using FluentAssertions;
using NSubstitute;
using WF.FraudService.Application.Contracts;
using WF.FraudService.Application.Contracts.DTOs;
using WF.FraudService.Application.Features.FraudChecks.Commands.CheckFraud;
using WF.FraudService.Application.Features.FraudChecks.Rules;
using WF.Shared.Contracts.Result;
using Xunit;

namespace WF.FraudService.UnitTests.Application.Features.FraudChecks.Rules;

public class RiskyHourFraudRuleTests
{
    private readonly IFraudRuleReadService _readService;
    private readonly ITimeProvider _timeProvider;
    private readonly RiskyHourFraudRule _rule;
    private readonly Bogus.Faker _faker;

    public RiskyHourFraudRuleTests()
    {
        _readService = Substitute.For<IFraudRuleReadService>();
        _timeProvider = Substitute.For<ITimeProvider>();
        _rule = new RiskyHourFraudRule(_readService, _timeProvider);
        _faker = new Bogus.Faker();
    }

    [Fact]
    public async Task EvaluateAsync_WhenNoActiveRules_ShouldReturnSuccess()
    {
        // Arrange
        var request = CreateValidRequest();
        _readService.GetActiveRiskyHourRulesAsync(Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<RiskyHourRuleDto>());

        // Act
        var result = await _rule.EvaluateAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_WhenCurrentTimeIsRisky_ShouldReturnFailure()
    {
        // Arrange
        var request = CreateValidRequest();
        var ruleDto = new RiskyHourRuleDto
        {
            Id = _faker.Random.Guid(),
            StartHour = 22,
            EndHour = 6,
            IsActive = true
        };

        var riskyTime = new DateTime(2024, 1, 1, 2, 0, 0, DateTimeKind.Utc); // 02:00 (in risky hours 22:00-06:00)

        _readService.GetActiveRiskyHourRulesAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { ruleDto });

        _timeProvider.UtcNow.Returns(riskyTime);

        // Act
        var result = await _rule.EvaluateAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("risky hours");
    }

    [Fact]
    public async Task EvaluateAsync_WhenCurrentTimeIsNotRisky_ShouldReturnSuccess()
    {
        // Arrange
        var request = CreateValidRequest();
        var ruleDto = new RiskyHourRuleDto
        {
            Id = _faker.Random.Guid(),
            StartHour = 22,
            EndHour = 6,
            IsActive = true
        };

        var safeTime = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc); // 12:00 (not in risky hours)

        _readService.GetActiveRiskyHourRulesAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { ruleDto });

        _timeProvider.UtcNow.Returns(safeTime);

        // Act
        var result = await _rule.EvaluateAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_WhenMultipleRules_ShouldCheckAll()
    {
        // Arrange
        var request = CreateValidRequest();
        var ruleDto1 = new RiskyHourRuleDto
        {
            Id = _faker.Random.Guid(),
            StartHour = 22,
            EndHour = 6,
            IsActive = true
        };

        var ruleDto2 = new RiskyHourRuleDto
        {
            Id = _faker.Random.Guid(),
            StartHour = 0,
            EndHour = 4,
            IsActive = true
        };

        var safeTime = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc); // 12:00 (not in any risky hours)

        _readService.GetActiveRiskyHourRulesAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { ruleDto1, ruleDto2 });

        _timeProvider.UtcNow.Returns(safeTime);

        // Act
        var result = await _rule.EvaluateAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_WhenTimeRangeCrossesMidnight_AndTimeInRange_ShouldReturnFailure()
    {
        // Arrange
        var request = CreateValidRequest();
        var ruleDto = new RiskyHourRuleDto
        {
            Id = _faker.Random.Guid(),
            StartHour = 22,
            EndHour = 6,
            IsActive = true
        };

        var riskyTime = new DateTime(2024, 1, 1, 23, 0, 0, DateTimeKind.Utc); // 23:00 (in risky hours 22:00-06:00)

        _readService.GetActiveRiskyHourRulesAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { ruleDto });

        _timeProvider.UtcNow.Returns(riskyTime);

        // Act
        var result = await _rule.EvaluateAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("risky hours");
    }

    private CheckFraudCommandInternal CreateValidRequest()
    {
        return new CheckFraudCommandInternal
        {
            CorrelationId = _faker.Random.Guid(),
            SenderCustomerId = _faker.Random.Guid(),
            ReceiverCustomerId = _faker.Random.Guid(),
            Amount = 100m,
            Currency = "USD",
            IpAddress = _faker.Internet.Ip()
        };
    }
}

