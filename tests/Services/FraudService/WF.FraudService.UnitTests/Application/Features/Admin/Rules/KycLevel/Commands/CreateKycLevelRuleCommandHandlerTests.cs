using FluentAssertions;
using NSubstitute;
using WF.FraudService.Application.Features.Admin.Rules.KycLevel.Commands.CreateKycLevelRule;
using WF.FraudService.Domain.Abstractions;
using WF.FraudService.Domain.Entities;
using WF.FraudService.Domain.ValueObjects;
using WF.Shared.Contracts.Abstractions;
using WF.Shared.Contracts.Enums;
using WF.Shared.Contracts.Result;
using Xunit;

namespace WF.FraudService.UnitTests.Application.Features.Admin.Rules.KycLevel.Commands.CreateKycLevelRule;

public class CreateKycLevelRuleCommandHandlerTests
{
    private readonly IKycLevelRuleRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateKycLevelRuleCommandHandler _handler;
    private readonly Bogus.Faker _faker;

    public CreateKycLevelRuleCommandHandlerTests()
    {
        _repository = Substitute.For<IKycLevelRuleRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new CreateKycLevelRuleCommandHandler(_repository, _unitOfWork);
        _faker = new Bogus.Faker();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateRuleSuccessfully()
    {
        // Arrange
        var requiredKycStatus = KycStatus.EmailVerified;
        var maxAllowedAmount = Money.Create(_faker.Random.Decimal(100, 10000)).Value;
        var description = _faker.Lorem.Sentence();
        var command = new CreateKycLevelRuleCommand(requiredKycStatus, maxAllowedAmount, description);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        await _repository.Received(1).AddAsync(Arg.Is<KycLevelRule>(r =>
            r.RequiredKycStatus == requiredKycStatus &&
            r.MaxAllowedAmount == maxAllowedAmount &&
            r.Description == description), Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryAddAndSaveChanges()
    {
        // Arrange
        var command = new CreateKycLevelRuleCommand(KycStatus.Unverified, null, null);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _repository.Received(1).AddAsync(Arg.Any<KycLevelRule>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

