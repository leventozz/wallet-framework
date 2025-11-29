using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using WF.Shared.Contracts.Abstractions;
using WF.Shared.Contracts.IntegrationEvents.Transaction;
using WF.Shared.Contracts.IntegrationEvents.Wallet;
using WF.WalletService.Application.Features.Wallets.Commands.DebitSenderWallet;
using WF.WalletService.Domain.Abstractions;
using WF.WalletService.Domain.Entities;
using WF.WalletService.Domain.ValueObjects;
using Xunit;

namespace WF.WalletService.UnitTests.Application.Features.Wallets.Commands.DebitSenderWallet;

public class DebitSenderWalletCommandHandlerTests
{
    private readonly IWalletRepository _walletRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIntegrationEventPublisher _eventPublisher;
    private readonly ILogger<DebitSenderWalletCommandHandler> _logger;
    private readonly DebitSenderWalletCommandHandler _handler;
    private readonly Bogus.Faker _faker;

    public DebitSenderWalletCommandHandlerTests()
    {
        _walletRepository = Substitute.For<IWalletRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _eventPublisher = Substitute.For<IIntegrationEventPublisher>();
        _logger = Substitute.For<ILogger<DebitSenderWalletCommandHandler>>();
        _handler = new DebitSenderWalletCommandHandler(
            _walletRepository,
            _unitOfWork,
            _eventPublisher,
            _logger);
        _faker = new Bogus.Faker();
    }

    private DebitSenderWalletCommand CreateValidCommand()
    {
        return new DebitSenderWalletCommand
        {
            CorrelationId = _faker.Random.Guid(),
            OwnerCustomerId = _faker.Random.Guid(),
            Amount = _faker.Random.Decimal(1, 1000),
            TransactionId = _faker.Random.Guid().ToString()
        };
    }

    private Wallet CreateValidWallet()
    {
        var customerId = _faker.Random.Guid();
        var walletNumber = _faker.Random.AlphaNumeric(8);
        var wallet = new Wallet(customerId, walletNumber);
        wallet.Deposit(Money.Create(1000m, "TRY").Value);
        return wallet;
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldDebitWalletSuccessfully()
    {
        // Arrange
        var command = CreateValidCommand();
        var wallet = CreateValidWallet();
        var initialBalance = wallet.Balance.Amount;

        _walletRepository.GetWalletByCustomerIdAsync(
            command.OwnerCustomerId,
            Arg.Any<CancellationToken>())
            .Returns(wallet);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        wallet.Balance.Amount.Should().Be(initialBalance - command.Amount);
        wallet.AvailableBalance.Amount.Should().Be(initialBalance - command.Amount);

        await _eventPublisher.Received(1).PublishAsync(
            Arg.Is<WalletDebitedEvent>(e =>
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

        _walletRepository.GetWalletByCustomerIdAsync(
            command.OwnerCustomerId,
            Arg.Any<CancellationToken>())
            .Returns((Wallet?)null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _eventPublisher.Received(1).PublishAsync(
            Arg.Is<WalletDebitFailedEvent>(e =>
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

        _walletRepository.GetWalletByCustomerIdAsync(
            command.OwnerCustomerId,
            Arg.Any<CancellationToken>())
            .Returns(wallet);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _eventPublisher.Received(1).PublishAsync(
            Arg.Is<WalletDebitFailedEvent>(e =>
                e.CorrelationId == command.CorrelationId),
            Arg.Any<CancellationToken>());


    }

    [Fact]
    public async Task Handle_WhenWithdrawFails_ShouldPublishFailureEvent()
    {
        // Arrange
        var command = CreateValidCommand();
        // Create wallet without deposit so it can be closed
        // Use command's OwnerCustomerId to match the repository call
        var walletNumber = _faker.Random.AlphaNumeric(8);
        var wallet = new Wallet(command.OwnerCustomerId, walletNumber);
        // Close wallet (balance is 0, so it can be closed)
        var closeResult = wallet.Close();
        closeResult.IsSuccess.Should().BeTrue("Wallet should be closed successfully when balance is 0");

        _walletRepository.GetWalletByCustomerIdAsync(
            command.OwnerCustomerId,
            Arg.Any<CancellationToken>())
            .Returns(wallet);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _eventPublisher.Received(1).PublishAsync(
            Arg.Is<WalletDebitFailedEvent>(e =>
                e.CorrelationId == command.CorrelationId &&
                e.Reason.Contains("closed")),
            Arg.Any<CancellationToken>());


    }

    [Fact]
    public async Task Handle_WhenInsufficientBalance_ShouldPublishFailureEvent()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { Amount = 2000m }; // More than balance
        var wallet = CreateValidWallet();

        _walletRepository.GetWalletByCustomerIdAsync(
            command.OwnerCustomerId,
            Arg.Any<CancellationToken>())
            .Returns(wallet);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _eventPublisher.Received(1).PublishAsync(
            Arg.Is<WalletDebitFailedEvent>(e =>
                e.CorrelationId == command.CorrelationId &&
                e.Reason.Contains("Insufficient")),
            Arg.Any<CancellationToken>());


    }

    [Fact]
    public async Task Handle_ShouldPublishWalletDebitedEvent()
    {
        // Arrange
        var command = CreateValidCommand();
        var wallet = CreateValidWallet();

        _walletRepository.GetWalletByCustomerIdAsync(
            command.OwnerCustomerId,
            Arg.Any<CancellationToken>())
            .Returns(wallet);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _eventPublisher.Received(1).PublishAsync(
            Arg.Is<WalletDebitedEvent>(e =>
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
        var expectedBalance = wallet.Balance.Amount - command.Amount;

        _walletRepository.GetWalletByCustomerIdAsync(
            command.OwnerCustomerId,
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
                e.NewBalance == expectedBalance),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldUpdateLastTransaction()
    {
        // Arrange
        var command = CreateValidCommand();
        var wallet = CreateValidWallet();

        _walletRepository.GetWalletByCustomerIdAsync(
            command.OwnerCustomerId,
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

        _walletRepository.GetWalletByCustomerIdAsync(
            command.OwnerCustomerId,
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

