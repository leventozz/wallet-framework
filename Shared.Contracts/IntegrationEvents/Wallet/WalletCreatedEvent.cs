using WF.Shared.Contracts.IntegrationEvents.Base;

namespace WF.Shared.Contracts.IntegrationEvents.Wallet
{
    public record WalletCreatedEvent(
        Guid WalletId,
        Guid CustomerId,
        decimal InitialBalance,
        string Currency) : WalletEventBase(WalletId)
    {
        public Guid CustomerId { get; init; } = CustomerId;
        public decimal InitialBalance { get; init; } = InitialBalance;
        public string Currency { get; set; } = Currency.ToString();
    }
}
