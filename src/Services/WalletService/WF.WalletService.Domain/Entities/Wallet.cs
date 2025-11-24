using WF.Shared.Contracts.Result;
using WF.WalletService.Domain.ValueObjects;

namespace WF.WalletService.Domain.Entities
{
    public class Wallet
    {
        public Guid Id { get; private set; }
        public Guid CustomerId { get; private set; }
        public string WalletNumber { get; private set; } = string.Empty;
        public Money Balance { get; private set; }
        public Money AvailableBalance { get; private set; } 
        public bool IsActive { get; private set; }
        public bool IsFrozen { get; private set; }
        public bool IsClosed { get; private set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? UpdatedAtUtc { get; private set; }
        public DateTime? ClosedAtUtc { get; private set; }
        public string? LastTransactionId { get; private set; }
        public DateTime? LastTransactionAtUtc { get; private set; }
        public Iban? Iban { get; private set; }
        public string? ExternalAccountRef { get; private set; }

        private Wallet() { }

        public Wallet(Guid customerId, string walletNumber, string? currency = null)
        {
            Id = Guid.NewGuid();
            CustomerId = customerId;
            WalletNumber = walletNumber;
            var currencyCode = currency ?? Shared.Contracts.Enums.Currency.TRY.ToString();
            Balance = Money.Create(0, currencyCode).Value;
            AvailableBalance = Money.Create(0, currencyCode).Value;
            IsActive = true;
            CreatedAtUtc = DateTime.UtcNow;
            UpdatedAtUtc = null;
            ClosedAtUtc = null;
            LastTransactionId = null;
            LastTransactionAtUtc = null;
            Iban = null;
            ExternalAccountRef = null;
        }

        public Result Deposit(Money depositMoney)
        {
            if (IsDeleted)
                return Result.Failure(Error.Conflict("Wallet.Deleted", "Cannot deposit to a deleted wallet."));

            if (IsClosed)
                return Result.Failure(Error.Conflict("Wallet.Closed", $"Wallet {Id} is closed"));

            if (IsFrozen)
                return Result.Failure(Error.Conflict("Wallet.Frozen", $"Wallet {Id} is frozen"));

            if (!IsActive)
                return Result.Failure(Error.Conflict("Wallet.NotActive", $"Wallet {Id} is not active"));

            if (depositMoney.Amount == 0)
                return Result.Failure(Error.Validation("Wallet.InvalidAmount", "The amount must be greater than zero."));

            Balance = Balance + depositMoney;
            AvailableBalance = AvailableBalance + depositMoney;
            UpdatedAtUtc = DateTime.UtcNow;
            return Result.Success();
        }

        public Result Withdraw(Money withdrawMoney)
        {
            if (IsDeleted)
                return Result.Failure(Error.Conflict("Wallet.Deleted", "Cannot withdraw from a deleted wallet."));

            if (IsClosed)
                return Result.Failure(Error.Conflict("Wallet.Closed", $"Wallet {Id} is closed"));

            if (IsFrozen)
                return Result.Failure(Error.Conflict("Wallet.Frozen", $"Wallet {Id} is frozen"));

            if (!IsActive)
                return Result.Failure(Error.Conflict("Wallet.NotActive", $"Wallet {Id} is not active"));

            if (withdrawMoney.Amount == 0)
                return Result.Failure(Error.Validation("Wallet.InvalidAmount", "The amount must be greater than zero."));

            if (Balance < withdrawMoney)
                return Result.Failure(Error.Conflict("Wallet.InsufficientBalance", $"Insufficient balance. Current balance: {Balance.Amount}, Requested amount: {withdrawMoney.Amount}"));

            if (AvailableBalance < withdrawMoney)
                return Result.Failure(Error.Conflict("Wallet.InsufficientAvailableBalance", $"Insufficient available balance. Current available balance: {AvailableBalance.Amount}, Requested amount: {withdrawMoney.Amount}"));

            Balance = Balance - withdrawMoney;
            AvailableBalance = AvailableBalance - withdrawMoney;
            UpdatedAtUtc = DateTime.UtcNow;
            return Result.Success();
        }

        public Result SetActive(bool isActive)
        {
            if (IsDeleted)
                return Result.Failure(Error.Conflict("Wallet.Deleted", "Cannot change active status of a deleted wallet."));

            if (IsClosed)
                return Result.Failure(Error.Conflict("Wallet.Closed", "Cannot change active status of a closed wallet."));

            IsActive = isActive;
            UpdatedAtUtc = DateTime.UtcNow;
            return Result.Success();
        }

        public Result Freeze()
        {
            if (IsDeleted)
                return Result.Failure(Error.Conflict("Wallet.Deleted", "Cannot freeze a deleted wallet."));

            if (IsClosed)
                return Result.Failure(Error.Conflict("Wallet.Closed", "Cannot freeze a closed wallet."));

            if (IsFrozen)
                return Result.Success();

            IsFrozen = true;
            UpdatedAtUtc = DateTime.UtcNow;
            return Result.Success();
        }

        public Result Unfreeze()
        {
            if (IsDeleted)
                return Result.Failure(Error.Conflict("Wallet.Deleted", "Cannot unfreeze a deleted wallet."));

            if (IsClosed)
                return Result.Failure(Error.Conflict("Wallet.Closed", "Cannot unfreeze a closed wallet."));

            if (!IsFrozen)
                return Result.Success();

            IsFrozen = false;
            UpdatedAtUtc = DateTime.UtcNow;
            return Result.Success();
        }

        public Result Close()
        {
            if (IsDeleted)
                return Result.Failure(Error.Conflict("Wallet.Deleted", "Cannot close a deleted wallet."));

            if (IsClosed)
                return Result.Success();

            if (Balance.Amount != 0)
                return Result.Failure(Error.Conflict("Wallet.NonZeroBalance", "Cannot close a wallet with non-zero balance."));

            IsClosed = true;
            IsActive = false;
            ClosedAtUtc = DateTime.UtcNow;
            UpdatedAtUtc = DateTime.UtcNow;
            return Result.Success();
        }

        public Result UpdateLastTransaction(string transactionId)
        {
            if (IsDeleted)
                return Result.Failure(Error.Conflict("Wallet.Deleted", "Cannot update transaction info of a deleted wallet."));

            LastTransactionId = transactionId;
            LastTransactionAtUtc = DateTime.UtcNow;
            UpdatedAtUtc = DateTime.UtcNow;
            return Result.Success();
        }

        public Result SoftDelete()
        {
            if (IsDeleted)
                return Result.Success();

            if (Balance.Amount != 0)
                return Result.Failure(Error.Conflict("Wallet.NonZeroBalance", "Cannot delete a wallet with non-zero balance."));

            IsDeleted = true;
            IsActive = false;
            IsClosed = true;
            ClosedAtUtc = DateTime.UtcNow;
            UpdatedAtUtc = DateTime.UtcNow;
            return Result.Success();
        }
    }
}
