using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using WF.FraudService.Application.Contracts;
using WF.FraudService.Application.Contracts.DTOs;
using WF.FraudService.Application.Features.FraudChecks.Commands.CheckFraud;
using WF.FraudService.Application.Features.FraudChecks.Rules;
using WF.Shared.Contracts.Abstractions;
using WF.Shared.Contracts.Dtos;
using WF.Shared.Contracts.Result;
using Xunit;

namespace WF.FraudService.UnitTests.Application.Features.FraudChecks.Rules;

public class AccountAgeFraudRuleTests
{
    private readonly IFraudRuleReadService _readService;
    private readonly ICustomerServiceApiClient _customerServiceApiClient;
    private readonly ITimeProvider _timeProvider;
    private readonly ILogger<AccountAgeFraudRule> _logger;
    private readonly AccountAgeFraudRule _rule;
    private readonly Bogus.Faker _faker;

    public AccountAgeFraudRuleTests()
    {
        _readService = Substitute.For<IFraudRuleReadService>();
        _customerServiceApiClient = Substitute.For<ICustomerServiceApiClient>();
        _timeProvider = Substitute.For<ITimeProvider>();
        _logger = Substitute.For<ILogger<AccountAgeFraudRule>>();
        _rule = new AccountAgeFraudRule(_readService, _customerServiceApiClient, _timeProvider, _logger);
        _faker = new Bogus.Faker();
    }

    [Fact]
    public async Task EvaluateAsync_WhenNoActiveRules_ShouldReturnSuccess()
    {
        // Arrange
        var request = CreateValidRequest();
        _readService.GetActiveAccountAgeRulesAsync(Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<AccountAgeRuleDto>());

        // Act
        var result = await _rule.EvaluateAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_WhenCustomerNotFound_ShouldReturnFailure()
    {
        // Arrange
        var request = CreateValidRequest();
        var ruleDto = new AccountAgeRuleDto
        {
            Id = _faker.Random.Guid(),
            MinAccountAgeDays = 30,
            MaxAllowedAmount = 1000m,
            IsActive = true
        };

        _readService.GetActiveAccountAgeRulesAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { ruleDto });

        _customerServiceApiClient.GetVerificationDataAsync(request.SenderCustomerId, Arg.Any<CancellationToken>())
            .Returns((CustomerVerificationDto?)null);

        // Act
        var result = await _rule.EvaluateAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Customer not found");
    }

    [Fact]
    public async Task EvaluateAsync_WhenAccountAgeIsSufficient_ShouldReturnSuccess()
    {
        // Arrange
        var request = CreateValidRequest(amount: 500m);
        var ruleDto = new AccountAgeRuleDto
        {
            Id = _faker.Random.Guid(),
            MinAccountAgeDays = 30,
            MaxAllowedAmount = 1000m,
            IsActive = true
        };

        var currentTime = new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc);
        var verificationData = new CustomerVerificationDto
        {
            Id = request.SenderCustomerId,
            CreatedAtUtc = currentTime.AddDays(-60), // 60 days old
            KycStatus = WF.Shared.Contracts.Enums.KycStatus.Unverified
        };

        _readService.GetActiveAccountAgeRulesAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { ruleDto });

        _customerServiceApiClient.GetVerificationDataAsync(request.SenderCustomerId, Arg.Any<CancellationToken>())
            .Returns(verificationData);

        _timeProvider.UtcNow.Returns(currentTime);

        // Act
        var result = await _rule.EvaluateAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_WhenAmountExceedsMaxForAccountAge_ShouldReturnFailure()
    {
        // Arrange
        var request = CreateValidRequest(amount: 2000m);
        var ruleDto = new AccountAgeRuleDto
        {
            Id = _faker.Random.Guid(),
            MinAccountAgeDays = 30,
            MaxAllowedAmount = 1000m,
            IsActive = true
        };

        var currentTime = new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc);
        var verificationData = new CustomerVerificationDto
        {
            Id = request.SenderCustomerId,
            CreatedAtUtc = currentTime.AddDays(-15), // 15 days old (less than 30)
            KycStatus = WF.Shared.Contracts.Enums.KycStatus.Unverified
        };

        _readService.GetActiveAccountAgeRulesAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { ruleDto });

        _customerServiceApiClient.GetVerificationDataAsync(request.SenderCustomerId, Arg.Any<CancellationToken>())
            .Returns(verificationData);

        _timeProvider.UtcNow.Returns(currentTime);

        // Act
        var result = await _rule.EvaluateAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("exceeds maximum allowed amount");
    }

    [Fact]
    public async Task EvaluateAsync_WhenMaxAllowedAmountIsNull_ShouldReturnSuccess()
    {
        // Arrange
        var request = CreateValidRequest(amount: 5000m);
        var ruleDto = new AccountAgeRuleDto
        {
            Id = _faker.Random.Guid(),
            MinAccountAgeDays = 30,
            MaxAllowedAmount = null,
            IsActive = true
        };

        var currentTime = new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc);
        var verificationData = new CustomerVerificationDto
        {
            Id = request.SenderCustomerId,
            CreatedAtUtc = currentTime.AddDays(-15),
            KycStatus = WF.Shared.Contracts.Enums.KycStatus.Unverified
        };

        _readService.GetActiveAccountAgeRulesAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { ruleDto });

        _customerServiceApiClient.GetVerificationDataAsync(request.SenderCustomerId, Arg.Any<CancellationToken>())
            .Returns(verificationData);

        _timeProvider.UtcNow.Returns(currentTime);

        // Act
        var result = await _rule.EvaluateAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    private CheckFraudCommandInternal CreateValidRequest(decimal amount = 100m)
    {
        return new CheckFraudCommandInternal
        {
            CorrelationId = _faker.Random.Guid(),
            SenderCustomerId = _faker.Random.Guid(),
            ReceiverCustomerId = _faker.Random.Guid(),
            Amount = amount,
            Currency = "USD",
            IpAddress = _faker.Internet.Ip()
        };
    }
}

