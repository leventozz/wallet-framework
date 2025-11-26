using FluentAssertions;
using NSubstitute;
using WF.FraudService.Application.Features.Admin.Rules.BlockedIp.Commands.CreateBlockedIpRule;
using WF.FraudService.Domain.Abstractions;
using WF.FraudService.Domain.Entities;
using WF.Shared.Contracts.Abstractions;
using WF.Shared.Contracts.Result;
using Xunit;

namespace WF.FraudService.UnitTests.Application.Features.Admin.Rules.BlockedIp.Commands.CreateBlockedIpRule;

public class CreateBlockedIpRuleCommandHandlerTests
{
    private readonly IBlockedIpRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateBlockedIpRuleCommandHandler _handler;
    private readonly Bogus.Faker _faker;

    public CreateBlockedIpRuleCommandHandlerTests()
    {
        _repository = Substitute.For<IBlockedIpRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new CreateBlockedIpRuleCommandHandler(_repository, _unitOfWork);
        _faker = new Bogus.Faker();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateRuleSuccessfully()
    {
        // Arrange
        var ipAddress = _faker.Internet.Ip();
        var reason = _faker.Lorem.Sentence();
        var expiresAtUtc = DateTime.UtcNow.AddDays(7);
        var command = new CreateBlockedIpRuleCommand(ipAddress, reason, expiresAtUtc);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        await _repository.Received(1).AddAsync(Arg.Is<BlockedIpRule>(r =>
            r.IpAddress.ToString() == ipAddress &&
            r.Reason == reason &&
            r.ExpiresAtUtc == expiresAtUtc), Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidIpAddress_ShouldReturnFailure()
    {
        // Arrange
        var invalidIp = "invalid-ip";
        var command = new CreateBlockedIpRuleCommand(invalidIp, null, null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("IpAddress.InvalidFormat");

        await _repository.DidNotReceive().AddAsync(Arg.Any<BlockedIpRule>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryAddAndSaveChanges()
    {
        // Arrange
        var ipAddress = _faker.Internet.Ip();
        var command = new CreateBlockedIpRuleCommand(ipAddress, null, null);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _repository.Received(1).AddAsync(Arg.Any<BlockedIpRule>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

