using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using WF.Shared.Contracts.Abstractions;
using WF.Shared.Contracts.IntegrationEvents.Transaction;
using WF.Shared.Contracts.IntegrationEvents.Wallet;
using WF.WalletService.Application.Features.Wallets.Commands.CreditWallet;
using WF.WalletService.Domain.Abstractions;
using WF.WalletService.Domain.Entities;
using WF.WalletService.Domain.ValueObjects;
using Xunit;

namespace WF.WalletService.UnitTests.Application.Features.Wallets.Commands.CreditWallet;

public class CreditWalletCommandHandlerTests
{
    private readonly IWalletRepository _walletRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIntegrationEventPublisher _eventPublisher;
    private readonly ILogger<CreditWalletCommandHandler> _logger;
    private readonly CreditWalletCommandHandler _handler;
    private readonly Bogus.Faker _faker;

    public CreditWalletCommandHandlerTests()
    {
        _walletRepository = Substitute.For<IWalletRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _eventPublisher = Substitute.For<IIntegrationEventPublisher>();
        _logger = Substitute.For<ILogger<CreditWalletCommandHandler>>();
        _handler = new CreditWalletCommandHandler(
            _walletRepository,
            _unitOfWork,
            _eventPublisher,
            _logger);
        _faker = new Bogus.Faker();
    }

    private CreditWalletCommand CreateValidCommand()
    {
        return new CreditWalletCommand
        {
            CorrelationId = _faker.Random.Guid(),
            WalletId = _faker.Random.Guid(),
            Amount = _faker.Random.Decimal(1, 1000),
            TransactionId = _faker.Random.Guid().ToString(),
            Currency = "TRY"
        };
    }

    private Wallet CreateValidWallet()
    {
        var customerId = _faker.Random.Guid();
        var walletNumber = _faker.Random.AlphaNumeric(8);
        return new Wallet(customerId, walletNumber);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreditWalletSuccessfully()
    {
        // Arrange
        var command = CreateValidCommand();
        var wallet = CreateValidWallet();

        _walletRepository.GetWalletByIdAsync(
            command.WalletId,
            Arg.Any<CancellationToken>())
            .Returns(wallet);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        wallet.Balance.Amount.Should().Be(command.Amount);
        wallet.AvailableBalance.Amount.Should().Be(command.Amount);

        await _eventPublisher.Received(1).PublishAsync(
            Arg.Is<WalletCreditedEvent>(e =>
                e.CorrelationId == command.CorrelationId &&
                e.WalletId == wallet.Id &&
                e.Amount == command.Amount),
            Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenWalletNotFound_ShouldPublishFailureEvent()
    {
        // Arrange
        var command = CreateValidCommand();

        _walletRepository.GetWalletByIdAsync(
            command.WalletId,
            Arg.Any<CancellationToken>())
            .Returns((Wallet?)null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _eventPublisher.Received(1).PublishAsync(
            Arg.Is<WalletCreditFailedEvent>(e =>
                e.CorrelationId == command.CorrelationId &&
                e.Reason.Contains("Wallet not found")),
            Arg.Any<CancellationToken>());



        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenMoneyCreationFails_ShouldPublishFailureEvent()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { Amount = -100m }; // Invalid amount
        var wallet = CreateValidWallet();

        _walletRepository.GetWalletByIdAsync(
            command.WalletId,
            Arg.Any<CancellationToken>())
            .Returns(wallet);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _eventPublisher.Received(1).PublishAsync(
            Arg.Is<WalletCreditFailedEvent>(e =>
                e.CorrelationId == command.CorrelationId),
            Arg.Any<CancellationToken>());


    }

    [Fact]
    public async Task Handle_WhenDepositFails_ShouldPublishFailureEvent()
    {
        // Arrange
        var command = CreateValidCommand();
        var wallet = CreateValidWallet();
        wallet.Close(); // Wallet is closed, deposit will fail

        _walletRepository.GetWalletByIdAsync(
            command.WalletId,
            Arg.Any<CancellationToken>())
            .Returns(wallet);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _eventPublisher.Received(1).PublishAsync(
            Arg.Is<WalletCreditFailedEvent>(e =>
                e.CorrelationId == command.CorrelationId &&
                e.Reason.Contains("closed")),
            Arg.Any<CancellationToken>());


    }

    [Fact]
    public async Task Handle_ShouldPublishWalletCreditedEvent()
    {
        // Arrange
        var command = CreateValidCommand();
        var wallet = CreateValidWallet();

        _walletRepository.GetWalletByIdAsync(
            command.WalletId,
            Arg.Any<CancellationToken>())
            .Returns(wallet);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _eventPublisher.Received(1).PublishAsync(
            Arg.Is<WalletCreditedEvent>(e =>
                e.CorrelationId == command.CorrelationId &&
                e.WalletId == wallet.Id &&
                e.Amount == command.Amount),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPublishWalletBalanceUpdatedEvent()
    {
        // Arrange
        var command = CreateValidCommand();
        var wallet = CreateValidWallet();

        _walletRepository.GetWalletByIdAsync(
            command.WalletId,
            Arg.Any<CancellationToken>())
            .Returns(wallet);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _eventPublisher.Received(1).PublishAsync(
            Arg.Is<WalletBalanceUpdatedEvent>(e =>
                e.WalletId == wallet.Id &&
                e.NewBalance == command.Amount),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldUpdateLastTransaction()
    {
        // Arrange
        var command = CreateValidCommand();
        var wallet = CreateValidWallet();

        _walletRepository.GetWalletByIdAsync(
            command.WalletId,
            Arg.Any<CancellationToken>())
            .Returns(wallet);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        wallet.LastTransactionId.Should().Be(command.TransactionId);
        wallet.LastTransactionAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldCallSaveChanges()
    {
        // Arrange
        var command = CreateValidCommand();
        var wallet = CreateValidWallet();

        _walletRepository.GetWalletByIdAsync(
            command.WalletId,
            Arg.Any<CancellationToken>())
            .Returns(wallet);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

