using FluentAssertions;
using WF.Shared.Contracts.Result;
using WF.WalletService.Domain.Entities;
using WF.WalletService.Domain.ValueObjects;
using Xunit;

namespace WF.WalletService.UnitTests.Domain;

public class WalletTests
{
    private readonly Bogus.Faker _faker = new();

    private Money CreateValidMoney(decimal amount = 100m, string currency = "TRY")
    {
        return Money.Create(amount, currency).Value;
    }

    private Wallet CreateValidWallet()
    {
        var customerId = _faker.Random.Guid();
        var walletNumber = _faker.Random.AlphaNumeric(8);
        return new Wallet(customerId, walletNumber);
    }

    [Fact]
    public void Constructor_WithValidData_ShouldCreateWallet()
    {
        // Arrange
        var customerId = _faker.Random.Guid();
        var walletNumber = _faker.Random.AlphaNumeric(8);

        // Act
        var wallet = new Wallet(customerId, walletNumber);

        // Assert
        wallet.Id.Should().NotBeEmpty();
        wallet.CustomerId.Should().Be(customerId);
        wallet.WalletNumber.Should().Be(walletNumber);
        wallet.Balance.Amount.Should().Be(0m);
        wallet.AvailableBalance.Amount.Should().Be(0m);
    }

    [Fact]
    public void Constructor_ShouldSetDefaultValues()
    {
        // Arrange
        var customerId = _faker.Random.Guid();
        var walletNumber = _faker.Random.AlphaNumeric(8);

        // Act
        var wallet = new Wallet(customerId, walletNumber);

        // Assert
        wallet.IsActive.Should().BeTrue();
        wallet.IsFrozen.Should().BeFalse();
        wallet.IsClosed.Should().BeFalse();
        wallet.IsDeleted.Should().BeFalse();
        wallet.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        wallet.UpdatedAtUtc.Should().BeNull();
        wallet.ClosedAtUtc.Should().BeNull();
        wallet.LastTransactionId.Should().BeNull();
        wallet.LastTransactionAtUtc.Should().BeNull();
        wallet.Iban.Should().BeNull();
        wallet.ExternalAccountRef.Should().BeNull();
    }

    [Fact]
    public void Deposit_WithValidAmount_ShouldIncreaseBalance()
    {
        // Arrange
        var wallet = CreateValidWallet();
        var depositAmount = CreateValidMoney(100m);

        // Act
        var result = wallet.Deposit(depositAmount);

        // Assert
        result.IsSuccess.Should().BeTrue();
        wallet.Balance.Amount.Should().Be(100m);
        wallet.AvailableBalance.Amount.Should().Be(100m);
    }

    [Fact]
    public void Deposit_WithZeroAmount_ShouldReturnFailure()
    {
        // Arrange
        var wallet = CreateValidWallet();
        var depositAmount = CreateValidMoney(0m);

        // Act
        var result = wallet.Deposit(depositAmount);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Wallet.InvalidAmount");
        result.Error.Message.Should().Be("The amount must be greater than zero.");
    }

    [Fact]
    public void Deposit_WhenDeleted_ShouldReturnFailure()
    {
        // Arrange
        var wallet = CreateValidWallet();
        wallet.SoftDelete();
        var depositAmount = CreateValidMoney(100m);

        // Act
        var result = wallet.Deposit(depositAmount);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Wallet.Deleted");
        result.Error.Message.Should().Be("Cannot deposit to a deleted wallet.");
    }

    [Fact]
    public void Deposit_WhenClosed_ShouldReturnFailure()
    {
        // Arrange
        var wallet = CreateValidWallet();
        wallet.Close();
        var depositAmount = CreateValidMoney(100m);

        // Act
        var result = wallet.Deposit(depositAmount);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Wallet.Closed");
    }

    [Fact]
    public void Deposit_WhenFrozen_ShouldReturnFailure()
    {
        // Arrange
        var wallet = CreateValidWallet();
        wallet.Freeze();
        var depositAmount = CreateValidMoney(100m);

        // Act
        var result = wallet.Deposit(depositAmount);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Wallet.Frozen");
    }

    [Fact]
    public void Deposit_WhenNotActive_ShouldReturnFailure()
    {
        // Arrange
        var wallet = CreateValidWallet();
        wallet.SetActive(false);
        var depositAmount = CreateValidMoney(100m);

        // Act
        var result = wallet.Deposit(depositAmount);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Wallet.NotActive");
    }

    [Fact]
    public void Deposit_ShouldUpdateTimestamp()
    {
        // Arrange
        var wallet = CreateValidWallet();
        var depositAmount = CreateValidMoney(100m);

        // Act
        var result = wallet.Deposit(depositAmount);

        // Assert
        result.IsSuccess.Should().BeTrue();
        wallet.UpdatedAtUtc.Should().NotBeNull();
        wallet.UpdatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Withdraw_WithValidAmount_ShouldDecreaseBalance()
    {
        // Arrange
        var wallet = CreateValidWallet();
        wallet.Deposit(CreateValidMoney(200m));
        var withdrawAmount = CreateValidMoney(100m);

        // Act
        var result = wallet.Withdraw(withdrawAmount);

        // Assert
        result.IsSuccess.Should().BeTrue();
        wallet.Balance.Amount.Should().Be(100m);
        wallet.AvailableBalance.Amount.Should().Be(100m);
    }

    [Fact]
    public void Withdraw_WithZeroAmount_ShouldReturnFailure()
    {
        // Arrange
        var wallet = CreateValidWallet();
        wallet.Deposit(CreateValidMoney(100m));
        var withdrawAmount = CreateValidMoney(0m);

        // Act
        var result = wallet.Withdraw(withdrawAmount);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Wallet.InvalidAmount");
    }

    [Fact]
    public void Withdraw_WhenDeleted_ShouldReturnFailure()
    {
        // Arrange
        var wallet = CreateValidWallet();
        // SoftDelete wallet without deposit (balance is 0, so it can be deleted)
        var deleteResult = wallet.SoftDelete();
        deleteResult.IsSuccess.Should().BeTrue("Wallet should be deleted successfully when balance is 0");
        var withdrawAmount = CreateValidMoney(50m);

        // Act
        var result = wallet.Withdraw(withdrawAmount);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Wallet.Deleted");
        result.Error.Message.Should().Be("Cannot withdraw from a deleted wallet.");
    }

    [Fact]
    public void Withdraw_WhenClosed_ShouldReturnFailure()
    {
        // Arrange
        var wallet = CreateValidWallet();
        // Close wallet without deposit (balance is 0, so it can be closed)
        var closeResult = wallet.Close();
        closeResult.IsSuccess.Should().BeTrue("Wallet should be closed successfully when balance is 0");
        var withdrawAmount = CreateValidMoney(50m);

        // Act
        var result = wallet.Withdraw(withdrawAmount);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Wallet.Closed");
    }

    [Fact]
    public void Withdraw_WhenFrozen_ShouldReturnFailure()
    {
        // Arrange
        var wallet = CreateValidWallet();
        wallet.Deposit(CreateValidMoney(100m));
        wallet.Freeze();
        var withdrawAmount = CreateValidMoney(50m);

        // Act
        var result = wallet.Withdraw(withdrawAmount);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Wallet.Frozen");
    }

    [Fact]
    public void Withdraw_WhenNotActive_ShouldReturnFailure()
    {
        // Arrange
        var wallet = CreateValidWallet();
        wallet.Deposit(CreateValidMoney(100m));
        wallet.SetActive(false);
        var withdrawAmount = CreateValidMoney(50m);

        // Act
        var result = wallet.Withdraw(withdrawAmount);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Wallet.NotActive");
    }

    [Fact]
    public void Withdraw_WithInsufficientBalance_ShouldReturnFailure()
    {
        // Arrange
        var wallet = CreateValidWallet();
        wallet.Deposit(CreateValidMoney(50m));
        var withdrawAmount = CreateValidMoney(100m);

        // Act
        var result = wallet.Withdraw(withdrawAmount);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Wallet.InsufficientBalance");
    }

    [Fact]
    public void Withdraw_ShouldUpdateTimestamp()
    {
        // Arrange
        var wallet = CreateValidWallet();
        wallet.Deposit(CreateValidMoney(200m));
        var withdrawAmount = CreateValidMoney(50m);

        // Act
        var result = wallet.Withdraw(withdrawAmount);

        // Assert
        result.IsSuccess.Should().BeTrue();
        wallet.UpdatedAtUtc.Should().NotBeNull();
        wallet.UpdatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void SetActive_ShouldToggleActiveStatus()
    {
        // Arrange
        var wallet = CreateValidWallet();
        wallet.IsActive.Should().BeTrue();

        // Act
        var result = wallet.SetActive(false);

        // Assert
        result.IsSuccess.Should().BeTrue();
        wallet.IsActive.Should().BeFalse();
        wallet.UpdatedAtUtc.Should().NotBeNull();
        wallet.UpdatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        // Act - Set back to active
        var result2 = wallet.SetActive(true);

        // Assert
        result2.IsSuccess.Should().BeTrue();
        wallet.IsActive.Should().BeTrue();
    }

    [Fact]
    public void SetActive_WhenDeleted_ShouldReturnFailure()
    {
        // Arrange
        var wallet = CreateValidWallet();
        wallet.SoftDelete();

        // Act
        var result = wallet.SetActive(true);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Wallet.Deleted");
        result.Error.Message.Should().Be("Cannot change active status of a deleted wallet.");
    }

    [Fact]
    public void SetActive_WhenClosed_ShouldReturnFailure()
    {
        // Arrange
        var wallet = CreateValidWallet();
        wallet.Close();

        // Act
        var result = wallet.SetActive(true);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Wallet.Closed");
        result.Error.Message.Should().Be("Cannot change active status of a closed wallet.");
    }

    [Fact]
    public void Freeze_WhenNotFrozen_ShouldSetFrozenTrue()
    {
        // Arrange
        var wallet = CreateValidWallet();

        // Act
        var result = wallet.Freeze();

        // Assert
        result.IsSuccess.Should().BeTrue();
        wallet.IsFrozen.Should().BeTrue();
        wallet.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void Freeze_WhenAlreadyFrozen_ShouldReturnSuccess()
    {
        // Arrange
        var wallet = CreateValidWallet();
        wallet.Freeze();
        var firstUpdatedAt = wallet.UpdatedAtUtc;

        // Act
        var result = wallet.Freeze();

        // Assert
        result.IsSuccess.Should().BeTrue();
        wallet.IsFrozen.Should().BeTrue();
        wallet.UpdatedAtUtc.Should().Be(firstUpdatedAt);
    }

    [Fact]
    public void Freeze_WhenDeleted_ShouldReturnFailure()
    {
        // Arrange
        var wallet = CreateValidWallet();
        wallet.SoftDelete();

        // Act
        var result = wallet.Freeze();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Wallet.Deleted");
        result.Error.Message.Should().Be("Cannot freeze a deleted wallet.");
    }

    [Fact]
    public void Freeze_WhenClosed_ShouldReturnFailure()
    {
        // Arrange
        var wallet = CreateValidWallet();
        wallet.Close();

        // Act
        var result = wallet.Freeze();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Wallet.Closed");
        result.Error.Message.Should().Be("Cannot freeze a closed wallet.");
    }

    [Fact]
    public void Unfreeze_WhenFrozen_ShouldSetFrozenFalse()
    {
        // Arrange
        var wallet = CreateValidWallet();
        wallet.Freeze();

        // Act
        var result = wallet.Unfreeze();

        // Assert
        result.IsSuccess.Should().BeTrue();
        wallet.IsFrozen.Should().BeFalse();
        wallet.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void Unfreeze_WhenNotFrozen_ShouldReturnSuccess()
    {
        // Arrange
        var wallet = CreateValidWallet();

        // Act
        var result = wallet.Unfreeze();

        // Assert
        result.IsSuccess.Should().BeTrue();
        wallet.IsFrozen.Should().BeFalse();
    }

    [Fact]
    public void Unfreeze_WhenDeleted_ShouldReturnFailure()
    {
        // Arrange
        var wallet = CreateValidWallet();
        wallet.Freeze();
        wallet.SoftDelete();

        // Act
        var result = wallet.Unfreeze();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Wallet.Deleted");
        result.Error.Message.Should().Be("Cannot unfreeze a deleted wallet.");
    }

    [Fact]
    public void Unfreeze_WhenClosed_ShouldReturnFailure()
    {
        // Arrange
        var wallet = CreateValidWallet();
        wallet.Freeze();
        wallet.Close();

        // Act
        var result = wallet.Unfreeze();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Wallet.Closed");
        result.Error.Message.Should().Be("Cannot unfreeze a closed wallet.");
    }

    [Fact]
    public void Close_WithZeroBalance_ShouldSetClosedTrue()
    {
        // Arrange
        var wallet = CreateValidWallet();

        // Act
        var result = wallet.Close();

        // Assert
        result.IsSuccess.Should().BeTrue();
        wallet.IsClosed.Should().BeTrue();
        wallet.IsActive.Should().BeFalse();
        wallet.ClosedAtUtc.Should().NotBeNull();
        wallet.ClosedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Close_WhenAlreadyClosed_ShouldReturnSuccess()
    {
        // Arrange
        var wallet = CreateValidWallet();
        wallet.Close();
        var firstClosedAt = wallet.ClosedAtUtc;

        // Act
        var result = wallet.Close();

        // Assert
        result.IsSuccess.Should().BeTrue();
        wallet.IsClosed.Should().BeTrue();
        wallet.ClosedAtUtc.Should().Be(firstClosedAt);
    }

    [Fact]
    public void Close_WhenDeleted_ShouldReturnFailure()
    {
        // Arrange
        var wallet = CreateValidWallet();
        wallet.SoftDelete();

        // Act
        var result = wallet.Close();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Wallet.Deleted");
        result.Error.Message.Should().Be("Cannot close a deleted wallet.");
    }

    [Fact]
    public void Close_WithNonZeroBalance_ShouldReturnFailure()
    {
        // Arrange
        var wallet = CreateValidWallet();
        wallet.Deposit(CreateValidMoney(100m));

        // Act
        var result = wallet.Close();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Wallet.NonZeroBalance");
        result.Error.Message.Should().Be("Cannot close a wallet with non-zero balance.");
    }

    [Fact]
    public void SoftDelete_WithZeroBalance_ShouldMarkAsDeleted()
    {
        // Arrange
        var wallet = CreateValidWallet();

        // Act
        var result = wallet.SoftDelete();

        // Assert
        result.IsSuccess.Should().BeTrue();
        wallet.IsDeleted.Should().BeTrue();
        wallet.IsActive.Should().BeFalse();
        wallet.IsClosed.Should().BeTrue();
        wallet.ClosedAtUtc.Should().NotBeNull();
        wallet.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void SoftDelete_WhenAlreadyDeleted_ShouldReturnSuccess()
    {
        // Arrange
        var wallet = CreateValidWallet();
        wallet.SoftDelete();
        var firstUpdatedAt = wallet.UpdatedAtUtc;

        // Act
        var result = wallet.SoftDelete();

        // Assert
        result.IsSuccess.Should().BeTrue();
        wallet.IsDeleted.Should().BeTrue();
        wallet.UpdatedAtUtc.Should().Be(firstUpdatedAt);
    }

    [Fact]
    public void SoftDelete_WithNonZeroBalance_ShouldReturnFailure()
    {
        // Arrange
        var wallet = CreateValidWallet();
        wallet.Deposit(CreateValidMoney(100m));

        // Act
        var result = wallet.SoftDelete();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Wallet.NonZeroBalance");
        result.Error.Message.Should().Be("Cannot delete a wallet with non-zero balance.");
    }

    [Fact]
    public void UpdateLastTransaction_WithValidId_ShouldUpdateFields()
    {
        // Arrange
        var wallet = CreateValidWallet();
        var transactionId = _faker.Random.Guid().ToString();

        // Act
        var result = wallet.UpdateLastTransaction(transactionId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        wallet.LastTransactionId.Should().Be(transactionId);
        wallet.LastTransactionAtUtc.Should().NotBeNull();
        wallet.LastTransactionAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        wallet.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void UpdateLastTransaction_WhenDeleted_ShouldReturnFailure()
    {
        // Arrange
        var wallet = CreateValidWallet();
        wallet.SoftDelete();
        var transactionId = _faker.Random.Guid().ToString();

        // Act
        var result = wallet.UpdateLastTransaction(transactionId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Wallet.Deleted");
        result.Error.Message.Should().Be("Cannot update transaction info of a deleted wallet.");
    }
}

