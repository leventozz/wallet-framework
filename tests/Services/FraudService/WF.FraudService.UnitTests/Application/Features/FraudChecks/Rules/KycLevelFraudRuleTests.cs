using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using WF.FraudService.Application.Contracts;
using WF.FraudService.Application.Contracts.DTOs;
using WF.FraudService.Application.Features.FraudChecks.Commands.CheckFraud;
using WF.FraudService.Application.Features.FraudChecks.Rules;
using WF.Shared.Contracts.Abstractions;
using WF.Shared.Contracts.Dtos;
using WF.Shared.Contracts.Enums;
using WF.Shared.Contracts.Result;
using Xunit;

namespace WF.FraudService.UnitTests.Application.Features.FraudChecks.Rules;

public class KycLevelFraudRuleTests
{
    private readonly IFraudRuleReadService _readService;
    private readonly ICustomerServiceApiClient _customerServiceApiClient;
    private readonly ILogger<KycLevelFraudRule> _logger;
    private readonly KycLevelFraudRule _rule;
    private readonly Bogus.Faker _faker;

    public KycLevelFraudRuleTests()
    {
        _readService = Substitute.For<IFraudRuleReadService>();
        _customerServiceApiClient = Substitute.For<ICustomerServiceApiClient>();
        _logger = Substitute.For<ILogger<KycLevelFraudRule>>();
        _rule = new KycLevelFraudRule(_readService, _customerServiceApiClient, _logger);
        _faker = new Bogus.Faker();
    }

    [Fact]
    public async Task EvaluateAsync_WhenNoActiveRules_ShouldReturnSuccess()
    {
        // Arrange
        var request = CreateValidRequest();
        _readService.GetActiveKycLevelRulesAsync(Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<KycLevelRuleDto>());

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
        var ruleDto = new KycLevelRuleDto
        {
            Id = _faker.Random.Guid(),
            RequiredKycStatus = KycStatus.EmailVerified,
            MaxAllowedAmount = 1000m,
            IsActive = true
        };

        _readService.GetActiveKycLevelRulesAsync(Arg.Any<CancellationToken>())
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
    public async Task EvaluateAsync_WhenKycLevelIsSufficient_ShouldReturnSuccess()
    {
        // Arrange
        var request = CreateValidRequest(amount: 500m);
        var ruleDto = new KycLevelRuleDto
        {
            Id = _faker.Random.Guid(),
            RequiredKycStatus = KycStatus.EmailVerified,
            MaxAllowedAmount = 1000m,
            IsActive = true
        };

        var verificationData = new CustomerVerificationDto
        {
            Id = request.SenderCustomerId,
            CreatedAtUtc = DateTime.UtcNow.AddDays(-60),
            KycStatus = KycStatus.VideoVerified // Higher than required
        };

        _readService.GetActiveKycLevelRulesAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { ruleDto });

        _customerServiceApiClient.GetVerificationDataAsync(request.SenderCustomerId, Arg.Any<CancellationToken>())
            .Returns(verificationData);

        // Act
        var result = await _rule.EvaluateAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_WhenAmountExceedsMaxForKycLevel_ShouldReturnFailure()
    {
        // Arrange
        var request = CreateValidRequest(amount: 2000m);
        var ruleDto = new KycLevelRuleDto
        {
            Id = _faker.Random.Guid(),
            RequiredKycStatus = KycStatus.EmailVerified,
            MaxAllowedAmount = 1000m,
            IsActive = true
        };

        var verificationData = new CustomerVerificationDto
        {
            Id = request.SenderCustomerId,
            CreatedAtUtc = DateTime.UtcNow.AddDays(-60),
            KycStatus = KycStatus.Unverified // Lower than required
        };

        _readService.GetActiveKycLevelRulesAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { ruleDto });

        _customerServiceApiClient.GetVerificationDataAsync(request.SenderCustomerId, Arg.Any<CancellationToken>())
            .Returns(verificationData);

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
        var ruleDto = new KycLevelRuleDto
        {
            Id = _faker.Random.Guid(),
            RequiredKycStatus = KycStatus.EmailVerified,
            MaxAllowedAmount = null,
            IsActive = true
        };

        var verificationData = new CustomerVerificationDto
        {
            Id = request.SenderCustomerId,
            CreatedAtUtc = DateTime.UtcNow.AddDays(-60),
            KycStatus = KycStatus.Unverified
        };

        _readService.GetActiveKycLevelRulesAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { ruleDto });

        _customerServiceApiClient.GetVerificationDataAsync(request.SenderCustomerId, Arg.Any<CancellationToken>())
            .Returns(verificationData);

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

