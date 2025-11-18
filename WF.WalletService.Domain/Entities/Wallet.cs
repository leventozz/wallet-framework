using WF.WalletService.Domain.Enums;
using WF.WalletService.Domain.Exceptions;

namespace WF.WalletService.Domain.Entities
{
    public class Wallet
    {
        public Guid Id { get; private set; }
        public Guid CustomerId { get; private set; }
        public string WalletNumber { get; private set; } = string.Empty;
        public string Currency { get; private set; } = string.Empty;
        public decimal Balance { get; private set; }
        public decimal AvailableBalance { get; private set; } 
        public bool IsActive { get; private set; }
        public bool IsFrozen { get; private set; }
        public bool IsClosed { get; private set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? UpdatedAtUtc { get; private set; }
        public DateTime? ClosedAtUtc { get; private set; }
        public string? LastTransactionId { get; private set; }
        public DateTime? LastTransactionAtUtc { get; private set; }
        public string? Iban { get; private set; }
        public string? ExternalAccountRef { get; private set; }

        private Wallet() { }

        public Wallet(Guid customerId, string walletNumber, string? currency = null)
        {
            Id = Guid.NewGuid();
            CustomerId = customerId;
            WalletNumber = walletNumber;
            Currency = currency ?? Enums.Currency.TRY.ToString();
            Balance = 0;
            AvailableBalance = 0;
            IsActive = true;
            CreatedAtUtc = DateTime.UtcNow;
            UpdatedAtUtc = null;
            ClosedAtUtc = null;
            LastTransactionId = null;
            LastTransactionAtUtc = null;
            Iban = null;
            ExternalAccountRef = null;
        }

        public void Deposit(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("The amount must be greater than zero.", nameof(amount));

            Balance += amount;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void Withdraw(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("The amount must be greater than zero.", nameof(amount));

            if (Balance < amount)
                throw new InsufficientBalanceException(Balance, amount);

            Balance -= amount;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void SetActive(bool isActive)
        {
            if (IsDeleted)
                throw new InvalidOperationException("Cannot change active status of a deleted wallet.");

            if (IsClosed)
                throw new InvalidOperationException("Cannot change active status of a closed wallet.");

            IsActive = isActive;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void Freeze()
        {
            if (IsDeleted)
                throw new InvalidOperationException("Cannot freeze a deleted wallet.");

            if (IsClosed)
                throw new InvalidOperationException("Cannot freeze a closed wallet.");

            if (IsFrozen)
                return;

            IsFrozen = true;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void Unfreeze()
        {
            if (IsDeleted)
                throw new InvalidOperationException("Cannot unfreeze a deleted wallet.");

            if (IsClosed)
                throw new InvalidOperationException("Cannot unfreeze a closed wallet.");

            if (!IsFrozen)
                return;

            IsFrozen = false;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void Close()
        {
            if (IsDeleted)
                throw new InvalidOperationException("Cannot close a deleted wallet.");

            if (IsClosed)
                return;

            if (Balance != 0)
                throw new InvalidOperationException("Cannot close a wallet with non-zero balance.");

            IsClosed = true;
            IsActive = false;
            ClosedAtUtc = DateTime.UtcNow;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void UpdateLastTransaction(string transactionId)
        {
            if (IsDeleted)
                throw new InvalidOperationException("Cannot update transaction info of a deleted wallet.");

            LastTransactionId = transactionId;
            LastTransactionAtUtc = DateTime.UtcNow;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void SoftDelete()
        {
            if (IsDeleted)
                return;

            if (Balance != 0)
                throw new InvalidOperationException("Cannot delete a wallet with non-zero balance.");

            IsDeleted = true;
            IsActive = false;
            IsClosed = true;
            ClosedAtUtc = DateTime.UtcNow;
            UpdatedAtUtc = DateTime.UtcNow;
        }
    }
}
