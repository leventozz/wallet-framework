using WF.WalletService.Domain.Exceptions;
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
            Balance = Money.Create(0, currencyCode);
            AvailableBalance = Money.Create(0, currencyCode);
            IsActive = true;
            CreatedAtUtc = DateTime.UtcNow;
            UpdatedAtUtc = null;
            ClosedAtUtc = null;
            LastTransactionId = null;
            LastTransactionAtUtc = null;
            Iban = null;
            ExternalAccountRef = null;
        }

        public void Deposit(Money depositMoney)
        {
            if (depositMoney.Amount == 0)
                throw new InvalidOperationException("The amount must be greater than zero.");

            Balance = Balance + depositMoney;
            AvailableBalance = AvailableBalance + depositMoney;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void Withdraw(Money withdrawMoney)
        {
            if (withdrawMoney.Amount == 0)
                throw new InvalidOperationException("The amount must be greater than zero.");

            if (Balance < withdrawMoney)
                throw new InsufficientBalanceException(Balance.Amount, withdrawMoney.Amount);

            if (AvailableBalance < withdrawMoney)
                throw new InsufficientBalanceException(AvailableBalance.Amount, withdrawMoney.Amount);

            Balance = Balance - withdrawMoney;
            AvailableBalance = AvailableBalance - withdrawMoney;
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

            if (Balance.Amount != 0)
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

            if (Balance.Amount != 0)
                throw new InvalidOperationException("Cannot delete a wallet with non-zero balance.");

            IsDeleted = true;
            IsActive = false;
            IsClosed = true;
            ClosedAtUtc = DateTime.UtcNow;
            UpdatedAtUtc = DateTime.UtcNow;
        }
    }
}
