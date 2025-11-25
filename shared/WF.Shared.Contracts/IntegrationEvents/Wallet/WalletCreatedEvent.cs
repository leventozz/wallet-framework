using WF.Shared.Contracts.IntegrationEvents.Base;

namespace WF.Shared.Contracts.IntegrationEvents.Wallet
{
    public record WalletCreatedEvent(
        Guid WalletId,
        Guid CustomerId,
        string WalletNumber,
        decimal InitialBalance,
        string Currency) : WalletEventBase(WalletId)
    {
        public Guid CustomerId { get; init; } = CustomerId;
        public string WalletNumber { get; init; } = WalletNumber;
        public decimal InitialBalance { get; init; } = InitialBalance;
        public string Currency { get; set; } = Currency.ToString();
    }
}
