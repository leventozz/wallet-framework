using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using WF.Shared.Contracts.Abstractions;
using WF.Shared.Contracts.IntegrationEvents.Transaction;
using WF.Shared.Contracts.IntegrationEvents.Wallet;
using WF.WalletService.Application.Features.Wallets.Commands.RefundSenderWallet;
using WF.WalletService.Domain.Abstractions;
using WF.WalletService.Domain.Entities;
using WF.WalletService.Domain.ValueObjects;
using Xunit;

namespace WF.WalletService.UnitTests.Application.Features.Wallets.Commands.RefundSenderWallet;

public class RefundSenderWalletCommandHandlerTests
{
    private readonly IWalletRepository _walletRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIntegrationEventPublisher _eventPublisher;
    private readonly ILogger<RefundSenderWalletCommandHandler> _logger;
    private readonly RefundSenderWalletCommandHandler _handler;
    private readonly Bogus.Faker _faker;

    public RefundSenderWalletCommandHandlerTests()
    {
        _walletRepository = Substitute.For<IWalletRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _eventPublisher = Substitute.For<IIntegrationEventPublisher>();
        _logger = Substitute.For<ILogger<RefundSenderWalletCommandHandler>>();
        _handler = new RefundSenderWalletCommandHandler(
            _walletRepository,
            _unitOfWork,
            _eventPublisher,
            _logger);
        _faker = new Bogus.Faker();
    }

    private RefundSenderWalletCommand CreateValidCommand()
    {
        return new RefundSenderWalletCommand
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
        return new Wallet(customerId, walletNumber);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldRefundWalletSuccessfully()
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
        wallet.Balance.Amount.Should().Be(initialBalance + command.Amount);
        wallet.AvailableBalance.Amount.Should().Be(initialBalance + command.Amount);

        await _eventPublisher.Received(1).PublishAsync(
            Arg.Is<SenderRefundedEvent>(e =>
                e.CorrelationId == command.CorrelationId &&
                e.WalletId == wallet.Id &&
                e.Amount == command.Amount),
            Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenWalletNotFound_ShouldLogErrorAndReturn()
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
        _logger.Received().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Wallet not found")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());



        await _eventPublisher.DidNotReceive().PublishAsync(
            Arg.Any<SenderRefundedEvent>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenWalletIsDeleted_ShouldLogWarningAndReturn()
    {
        // Arrange
        var command = CreateValidCommand();
        var wallet = CreateValidWallet();
        wallet.SoftDelete();

        _walletRepository.GetWalletByCustomerIdAsync(
            command.OwnerCustomerId,
            Arg.Any<CancellationToken>())
            .Returns(wallet);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _logger.Received().Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("deleted")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());



        await _eventPublisher.DidNotReceive().PublishAsync(
            Arg.Any<SenderRefundedEvent>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenMoneyCreationFails_ShouldLogErrorAndReturn()
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
        _logger.Received().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Invalid amount")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());


    }

    [Fact]
    public async Task Handle_WhenDepositFails_ShouldLogErrorAndReturn()
    {
        // Arrange
        var command = CreateValidCommand();
        var wallet = CreateValidWallet();
        wallet.Close(); // Wallet is closed, deposit will fail

        _walletRepository.GetWalletByCustomerIdAsync(
            command.OwnerCustomerId,
            Arg.Any<CancellationToken>())
            .Returns(wallet);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _logger.Received().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Failed to refund")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());


    }

    [Fact]
    public async Task Handle_ShouldPublishSenderRefundedEvent()
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
            Arg.Is<SenderRefundedEvent>(e =>
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
        var expectedBalance = wallet.Balance.Amount + command.Amount;

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

