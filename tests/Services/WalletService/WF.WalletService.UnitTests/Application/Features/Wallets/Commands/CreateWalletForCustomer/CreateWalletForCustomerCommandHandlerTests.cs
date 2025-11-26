using FluentAssertions;
using NSubstitute;
using WF.Shared.Contracts.Abstractions;
using WF.Shared.Contracts.IntegrationEvents.Wallet;
using WF.WalletService.Application.Features.Wallets.Commands.CreateWalletForCustomer;
using WF.WalletService.Domain.Abstractions;
using WF.WalletService.Domain.Entities;
using Xunit;

namespace WF.WalletService.UnitTests.Application.Features.Wallets.Commands.CreateWalletForCustomer;

public class CreateWalletForCustomerCommandHandlerTests
{
    private readonly IWalletRepository _walletRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIntegrationEventPublisher _eventPublisher;
    private readonly CreateWalletForCustomerCommandHandler _handler;
    private readonly Bogus.Faker _faker;

    public CreateWalletForCustomerCommandHandlerTests()
    {
        _walletRepository = Substitute.For<IWalletRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _eventPublisher = Substitute.For<IIntegrationEventPublisher>();
        _handler = new CreateWalletForCustomerCommandHandler(
            _walletRepository,
            _unitOfWork,
            _eventPublisher);
        _faker = new Bogus.Faker();
    }

    private CreateWalletForCustomerCommand CreateValidCommand()
    {
        return new CreateWalletForCustomerCommand
        {
            CustomerId = _faker.Random.Guid()
        };
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateWalletSuccessfully()
    {
        // Arrange
        var command = CreateValidCommand();

        _walletRepository.IsWalletNumberUniqueAsync(
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        await _walletRepository.Received().IsWalletNumberUniqueAsync(
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());

        await _walletRepository.Received(1).AddWalletAsync(
            Arg.Is<Wallet>(w => w.CustomerId == command.CustomerId),
            Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldGenerateUniqueWalletNumber()
    {
        // Arrange
        var command = CreateValidCommand();

        _walletRepository.IsWalletNumberUniqueAsync(
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _walletRepository.Received().IsWalletNumberUniqueAsync(
            Arg.Is<string>(wn => wn.Length == 8 && wn.All(char.IsDigit)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenWalletNumberGenerationFails_ShouldThrowException()
    {
        // Arrange
        var command = CreateValidCommand();

        _walletRepository.IsWalletNumberUniqueAsync(
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(false); // All attempts return false (not unique)

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Unable to generate a unique wallet number after 5 attempts*");

        await _walletRepository.DidNotReceive().AddWalletAsync(
            Arg.Any<Wallet>(),
            Arg.Any<CancellationToken>());

        await _eventPublisher.DidNotReceive().PublishAsync(
            Arg.Any<WalletCreatedEvent>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPublishWalletCreatedEvent()
    {
        // Arrange
        var command = CreateValidCommand();

        _walletRepository.IsWalletNumberUniqueAsync(
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _eventPublisher.Received(1).PublishAsync(
            Arg.Is<WalletCreatedEvent>(e =>
                e.WalletId == result.Value &&
                e.CustomerId == command.CustomerId &&
                e.InitialBalance == 0m),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCallSaveChanges()
    {
        // Arrange
        var command = CreateValidCommand();

        _walletRepository.IsWalletNumberUniqueAsync(
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldRetryWalletNumberGeneration_WhenNotUnique()
    {
        // Arrange
        var command = CreateValidCommand();

        // First 2 attempts return false, third returns true
        _walletRepository.IsWalletNumberUniqueAsync(
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(false, false, true);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Should have been called 3 times (2 failures + 1 success)
        await _walletRepository.Received(3).IsWalletNumberUniqueAsync(
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());

        await _walletRepository.Received(1).AddWalletAsync(
            Arg.Any<Wallet>(),
            Arg.Any<CancellationToken>());
    }
}

