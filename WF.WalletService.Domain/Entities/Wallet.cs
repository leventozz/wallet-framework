using WF.WalletService.Domain.Enums;

namespace WF.WalletService.Domain.Entities
{
    public class Wallet
    {
        public Guid Id { get; private set; }
        public Guid CustomerId { get; private set; }
        public string WalletNumber { get; private set; } = string.Empty;
        public Currency Currency { get; private set; }
        public decimal Balance { get; private set; }
        public decimal AvailableBalance { get; private set; } 
        public bool IsActive { get; private set; }
        public bool IsFrozen { get; private set; }
        public bool IsClosed { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? UpdatedAtUtc { get; private set; }
        public DateTime? ClosedAtUtc { get; private set; }
        public string? LastTransactionId { get; private set; }
        public DateTime? LastTransactionAtUtc { get; private set; }
        public string? Iban { get; private set; }
        public string? ExternalAccountRef { get; private set; }

        private Wallet() { }

        public Wallet(Guid customerId, string walletNumber, Currency currency = Currency.TRY)
        {
            Id = Guid.NewGuid();
            CustomerId = customerId;
            WalletNumber = walletNumber;
            Currency = currency;
            Balance = 0;
            AvailableBalance = 0;
            IsActive = true;
            IsFrozen = false;
            IsClosed = false;
            CreatedAtUtc = DateTime.UtcNow;
            UpdatedAtUtc = null;
            ClosedAtUtc = null;
            LastTransactionId = null;
            LastTransactionAtUtc = null;
            Iban = null;
            ExternalAccountRef = null;
        }
    }
}
