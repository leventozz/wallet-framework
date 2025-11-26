using FluentAssertions;
using NSubstitute;
using WF.Shared.Contracts.Abstractions;
using WF.Shared.Contracts.Result;
using WF.WalletService.Application.Features.Admin.Commands.CloseWallet;
using WF.WalletService.Domain.Abstractions;
using WF.WalletService.Domain.Entities;
using WF.WalletService.Domain.ValueObjects;
using Xunit;

namespace WF.WalletService.UnitTests.Application.Features.Admin.Commands.CloseWallet;

public class CloseWalletCommandHandlerTests
{
    private readonly IWalletRepository _walletRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CloseWalletCommandHandler _handler;
    private readonly Bogus.Faker _faker;

    public CloseWalletCommandHandlerTests()
    {
        _walletRepository = Substitute.For<IWalletRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new CloseWalletCommandHandler(
            _walletRepository,
            _unitOfWork);
        _faker = new Bogus.Faker();
    }

    private CloseWalletCommand CreateValidCommand()
    {
        return new CloseWalletCommand(_faker.Random.Guid());
    }

    private Wallet CreateValidWallet()
    {
        var customerId = _faker.Random.Guid();
        var walletNumber = _faker.Random.AlphaNumeric(8);
        return new Wallet(customerId, walletNumber);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCloseWalletSuccessfully()
    {
        // Arrange
        var command = CreateValidCommand();
        var wallet = CreateValidWallet();

        _walletRepository.GetWalletByIdForUpdateAsync(
            command.WalletId,
            Arg.Any<CancellationToken>())
            .Returns(wallet);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        wallet.IsClosed.Should().BeTrue();
        wallet.IsActive.Should().BeFalse();
        wallet.ClosedAtUtc.Should().NotBeNull();

        await _walletRepository.Received(1).UpdateWalletAsync(
            wallet,
            Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenWalletNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        var command = CreateValidCommand();

        _walletRepository.GetWalletByIdForUpdateAsync(
            command.WalletId,
            Arg.Any<CancellationToken>())
            .Returns((Wallet?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotFound");
        result.Error.Message.Should().Contain("Wallet");
        result.Error.Message.Should().Contain("not found");

        await _walletRepository.DidNotReceive().UpdateWalletAsync(
            Arg.Any<Wallet>(),
            Arg.Any<CancellationToken>());

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenWalletAlreadyClosed_ShouldReturnSuccess()
    {
        // Arrange
        var command = CreateValidCommand();
        var wallet = CreateValidWallet();
        wallet.Close();

        _walletRepository.GetWalletByIdForUpdateAsync(
            command.WalletId,
            Arg.Any<CancellationToken>())
            .Returns(wallet);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        wallet.IsClosed.Should().BeTrue();

        await _walletRepository.Received(1).UpdateWalletAsync(
            wallet,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenWalletHasNonZeroBalance_ShouldReturnFailure()
    {
        // Arrange
        var command = CreateValidCommand();
        var wallet = CreateValidWallet();
        wallet.Deposit(Money.Create(100m, "TRY").Value);

        _walletRepository.GetWalletByIdForUpdateAsync(
            command.WalletId,
            Arg.Any<CancellationToken>())
            .Returns(wallet);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Wallet.NonZeroBalance");
        result.Error.Message.Should().Contain("non-zero balance");

        await _walletRepository.DidNotReceive().UpdateWalletAsync(
            Arg.Any<Wallet>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCallSaveChanges()
    {
        // Arrange
        var command = CreateValidCommand();
        var wallet = CreateValidWallet();

        _walletRepository.GetWalletByIdForUpdateAsync(
            command.WalletId,
            Arg.Any<CancellationToken>())
            .Returns(wallet);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

