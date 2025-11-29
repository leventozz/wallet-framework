using FluentAssertions;
using NSubstitute;
using WF.Shared.Contracts.Abstractions;
using WF.Shared.Contracts.Result;
using WF.WalletService.Application.Features.Admin.Commands.FreezeWallet;
using WF.WalletService.Domain.Abstractions;
using WF.WalletService.Domain.Entities;
using Xunit;

namespace WF.WalletService.UnitTests.Application.Features.Admin.Commands.FreezeWallet;

public class FreezeWalletCommandHandlerTests
{
    private readonly IWalletRepository _walletRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly FreezeWalletCommandHandler _handler;
    private readonly Bogus.Faker _faker;

    public FreezeWalletCommandHandlerTests()
    {
        _walletRepository = Substitute.For<IWalletRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new FreezeWalletCommandHandler(
            _walletRepository,
            _unitOfWork);
        _faker = new Bogus.Faker();
    }

    private FreezeWalletCommand CreateValidCommand()
    {
        return new FreezeWalletCommand(_faker.Random.Guid());
    }

    private Wallet CreateValidWallet()
    {
        var customerId = _faker.Random.Guid();
        var walletNumber = _faker.Random.AlphaNumeric(8);
        return new Wallet(customerId, walletNumber);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldFreezeWalletSuccessfully()
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
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        wallet.IsFrozen.Should().BeTrue();

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

        _walletRepository.GetWalletByIdAsync(
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
    public async Task Handle_WhenWalletAlreadyFrozen_ShouldReturnSuccess()
    {
        // Arrange
        var command = CreateValidCommand();
        var wallet = CreateValidWallet();
        wallet.Freeze();

        _walletRepository.GetWalletByIdAsync(
            command.WalletId,
            Arg.Any<CancellationToken>())
            .Returns(wallet);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        wallet.IsFrozen.Should().BeTrue();

        await _walletRepository.Received(1).UpdateWalletAsync(
            wallet,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenWalletIsClosed_ShouldReturnFailure()
    {
        // Arrange
        var command = CreateValidCommand();
        var wallet = CreateValidWallet();
        wallet.Close();

        _walletRepository.GetWalletByIdAsync(
            command.WalletId,
            Arg.Any<CancellationToken>())
            .Returns(wallet);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Wallet.Closed");

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

        _walletRepository.GetWalletByIdAsync(
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

