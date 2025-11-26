using FluentAssertions;
using NSubstitute;
using WF.FraudService.Application.Features.Admin.Rules.AccountAge.Commands.UpdateAccountAgeRule;
using WF.FraudService.Domain.Abstractions;
using WF.FraudService.Domain.Entities;
using WF.FraudService.Domain.ValueObjects;
using WF.Shared.Contracts.Abstractions;
using WF.Shared.Contracts.Result;
using Xunit;

namespace WF.FraudService.UnitTests.Application.Features.Admin.Rules.AccountAge.Commands.UpdateAccountAgeRule;

public class UpdateAccountAgeRuleCommandHandlerTests
{
    private readonly IAccountAgeRuleRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UpdateAccountAgeRuleCommandHandler _handler;
    private readonly Bogus.Faker _faker;

    public UpdateAccountAgeRuleCommandHandlerTests()
    {
        _repository = Substitute.For<IAccountAgeRuleRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new UpdateAccountAgeRuleCommandHandler(_repository, _unitOfWork);
        _faker = new Bogus.Faker();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateRuleSuccessfully()
    {
        // Arrange
        var ruleId = _faker.Random.Guid();
        var existingRule = AccountAgeRule.Create(30, null, null).Value;
        var command = new UpdateAccountAgeRuleCommand(
            ruleId,
            60,
            Money.Create(5000m).Value,
            "Updated description",
            true);

        _repository.GetByIdAsync(ruleId, Arg.Any<CancellationToken>())
            .Returns(existingRule);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingRule.MinAccountAgeDays.Should().Be(60);
        existingRule.MaxAllowedAmount.Should().Be(Money.Create(5000m).Value);
        existingRule.Description.Should().Be("Updated description");
        existingRule.IsActive.Should().BeTrue();

        await _repository.Received(1).UpdateAsync(existingRule, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRuleNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        var ruleId = _faker.Random.Guid();
        var command = new UpdateAccountAgeRuleCommand(ruleId, 60, null, null, true);

        _repository.GetByIdAsync(ruleId, Arg.Any<CancellationToken>())
            .Returns((AccountAgeRule?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotFound");

        await _repository.DidNotReceive().UpdateAsync(Arg.Any<AccountAgeRule>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNegativeMinAccountAgeDays_ShouldReturnFailure()
    {
        // Arrange
        var ruleId = _faker.Random.Guid();
        var existingRule = AccountAgeRule.Create(30, null, null).Value;
        var command = new UpdateAccountAgeRuleCommand(ruleId, -10, null, null, true);

        _repository.GetByIdAsync(ruleId, Arg.Any<CancellationToken>())
            .Returns(existingRule);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AccountAgeRule.NegativeMinAccountAgeDays");

        await _repository.DidNotReceive().UpdateAsync(Arg.Any<AccountAgeRule>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithIsActiveTrue_ShouldActivateRule()
    {
        // Arrange
        var ruleId = _faker.Random.Guid();
        var existingRule = AccountAgeRule.Create(30, null, null).Value;
        existingRule.Deactivate();
        var command = new UpdateAccountAgeRuleCommand(ruleId, null, null, null, true);

        _repository.GetByIdAsync(ruleId, Arg.Any<CancellationToken>())
            .Returns(existingRule);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingRule.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithIsActiveFalse_ShouldDeactivateRule()
    {
        // Arrange
        var ruleId = _faker.Random.Guid();
        var existingRule = AccountAgeRule.Create(30, null, null).Value;
        var command = new UpdateAccountAgeRuleCommand(ruleId, null, null, null, false);

        _repository.GetByIdAsync(ruleId, Arg.Any<CancellationToken>())
            .Returns(existingRule);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingRule.IsActive.Should().BeFalse();
    }
}

