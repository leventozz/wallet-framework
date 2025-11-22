using WF.Shared.Contracts.IntegrationEvents.Base;

namespace WF.Shared.Contracts.IntegrationEvents.Wallet
{
    public record WalletBalanceUpdatedEvent(
        Guid WalletId,
        decimal NewBalance,
        DateTime TransactionDate) : WalletEventBase(WalletId)
    {
        public decimal NewBalance { get; init; } = NewBalance;
    }
}
