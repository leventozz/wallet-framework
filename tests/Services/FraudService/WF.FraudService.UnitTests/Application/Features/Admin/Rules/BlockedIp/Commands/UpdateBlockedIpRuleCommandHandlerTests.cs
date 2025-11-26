using FluentAssertions;
using NSubstitute;
using WF.FraudService.Application.Features.Admin.Rules.BlockedIp.Commands.UpdateBlockedIpRule;
using WF.FraudService.Domain.Abstractions;
using WF.FraudService.Domain.Entities;
using WF.FraudService.Domain.ValueObjects;
using WF.Shared.Contracts.Abstractions;
using WF.Shared.Contracts.Result;
using Xunit;

namespace WF.FraudService.UnitTests.Application.Features.Admin.Rules.BlockedIp.Commands.UpdateBlockedIpRule;

public class UpdateBlockedIpRuleCommandHandlerTests
{
    private readonly IBlockedIpRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UpdateBlockedIpRuleCommandHandler _handler;
    private readonly Bogus.Faker _faker;

    public UpdateBlockedIpRuleCommandHandlerTests()
    {
        _repository = Substitute.For<IBlockedIpRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new UpdateBlockedIpRuleCommandHandler(_repository, _unitOfWork);
        _faker = new Bogus.Faker();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateRuleSuccessfully()
    {
        // Arrange
        var ruleId = _faker.Random.Guid();
        var ipAddress = IpAddress.Create(_faker.Internet.Ip()).Value;
        var existingRule = BlockedIpRule.Create(ipAddress, "Old reason", null).Value;
        var command = new UpdateBlockedIpRuleCommand(ruleId, "New reason", true);

        _repository.GetByIdAsync(ruleId, Arg.Any<CancellationToken>())
            .Returns(existingRule);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingRule.Reason.Should().Be("New reason");
        existingRule.IsActive.Should().BeTrue();

        await _repository.Received(1).UpdateAsync(existingRule, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRuleNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        var ruleId = _faker.Random.Guid();
        var command = new UpdateBlockedIpRuleCommand(ruleId, "New reason", true);

        _repository.GetByIdAsync(ruleId, Arg.Any<CancellationToken>())
            .Returns((BlockedIpRule?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotFound");

        await _repository.DidNotReceive().UpdateAsync(Arg.Any<BlockedIpRule>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithIsActiveTrue_ShouldActivateRule()
    {
        // Arrange
        var ruleId = _faker.Random.Guid();
        var ipAddress = IpAddress.Create(_faker.Internet.Ip()).Value;
        var existingRule = BlockedIpRule.Create(ipAddress, null, null).Value;
        existingRule.Deactivate();
        var command = new UpdateBlockedIpRuleCommand(ruleId, null, true);

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
        var ipAddress = IpAddress.Create(_faker.Internet.Ip()).Value;
        var existingRule = BlockedIpRule.Create(ipAddress, null, null).Value;
        var command = new UpdateBlockedIpRuleCommand(ruleId, null, false);

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

