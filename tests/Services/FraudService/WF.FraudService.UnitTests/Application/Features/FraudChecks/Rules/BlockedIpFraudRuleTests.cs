using FluentAssertions;
using NSubstitute;
using WF.FraudService.Application.Contracts;
using WF.FraudService.Application.Contracts.DTOs;
using WF.FraudService.Application.Features.FraudChecks.Commands.CheckFraud;
using WF.FraudService.Application.Features.FraudChecks.Rules;
using WF.Shared.Contracts.Result;
using Xunit;

namespace WF.FraudService.UnitTests.Application.Features.FraudChecks.Rules;

public class BlockedIpFraudRuleTests
{
    private readonly IFraudRuleReadService _readService;
    private readonly ITimeProvider _timeProvider;
    private readonly BlockedIpFraudRule _rule;
    private readonly Bogus.Faker _faker;

    public BlockedIpFraudRuleTests()
    {
        _readService = Substitute.For<IFraudRuleReadService>();
        _timeProvider = Substitute.For<ITimeProvider>();
        _rule = new BlockedIpFraudRule(_readService, _timeProvider);
        _faker = new Bogus.Faker();
    }

    [Fact]
    public async Task EvaluateAsync_WhenIpAddressIsEmpty_ShouldReturnSuccess()
    {
        // Arrange
        var request = CreateValidRequest();
        request = request with { IpAddress = string.Empty };

        // Act
        var result = await _rule.EvaluateAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _readService.DidNotReceive().GetBlockedIpRuleAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EvaluateAsync_WhenIpAddressIsNull_ShouldReturnSuccess()
    {
        // Arrange
        var request = CreateValidRequest();
        request = request with { IpAddress = null };

        // Act
        var result = await _rule.EvaluateAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_WhenIpIsBlocked_ShouldReturnFailure()
    {
        // Arrange
        var ipAddress = _faker.Internet.Ip();
        var request = CreateValidRequest();
        request = request with { IpAddress = ipAddress };

        var currentTime = new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc);
        var blockedIpRule = new BlockedIpRuleDto
        {
            Id = _faker.Random.Guid(),
            IpAddress = ipAddress,
            IsActive = true,
            ExpiresAtUtc = currentTime.AddDays(7)
        };

        _readService.GetBlockedIpRuleAsync(ipAddress, Arg.Any<CancellationToken>())
            .Returns(blockedIpRule);

        _timeProvider.UtcNow.Returns(currentTime);

        // Act
        var result = await _rule.EvaluateAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("is blocked");
    }

    [Fact]
    public async Task EvaluateAsync_WhenIpIsNotBlocked_ShouldReturnSuccess()
    {
        // Arrange
        var ipAddress = _faker.Internet.Ip();
        var request = CreateValidRequest();
        request = request with { IpAddress = ipAddress };

        _readService.GetBlockedIpRuleAsync(ipAddress, Arg.Any<CancellationToken>())
            .Returns((BlockedIpRuleDto?)null);

        // Act
        var result = await _rule.EvaluateAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_WhenIpBlockExpired_ShouldReturnSuccess()
    {
        // Arrange
        var ipAddress = _faker.Internet.Ip();
        var request = CreateValidRequest();
        request = request with { IpAddress = ipAddress };

        var currentTime = new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc);
        var expiredBlockedIpRule = new BlockedIpRuleDto
        {
            Id = _faker.Random.Guid(),
            IpAddress = ipAddress,
            IsActive = true,
            ExpiresAtUtc = currentTime.AddDays(-1) // Expired
        };

        _readService.GetBlockedIpRuleAsync(ipAddress, Arg.Any<CancellationToken>())
            .Returns(expiredBlockedIpRule);

        _timeProvider.UtcNow.Returns(currentTime);

        // Act
        var result = await _rule.EvaluateAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_WhenIpIsInactive_ShouldReturnSuccess()
    {
        // Arrange
        var ipAddress = _faker.Internet.Ip();
        var request = CreateValidRequest();
        request = request with { IpAddress = ipAddress };

        var currentTime = new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc);
        var inactiveBlockedIpRule = new BlockedIpRuleDto
        {
            Id = _faker.Random.Guid(),
            IpAddress = ipAddress,
            IsActive = false,
            ExpiresAtUtc = currentTime.AddDays(7)
        };

        _readService.GetBlockedIpRuleAsync(ipAddress, Arg.Any<CancellationToken>())
            .Returns(inactiveBlockedIpRule);

        _timeProvider.UtcNow.Returns(currentTime);

        // Act
        var result = await _rule.EvaluateAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
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

