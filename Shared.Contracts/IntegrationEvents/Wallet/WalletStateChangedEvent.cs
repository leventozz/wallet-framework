using WF.Shared.Contracts.IntegrationEvents.Base;
using WF.Shared.Contracts.IntegrationEvents.Enum;

namespace WF.Shared.Contracts.IntegrationEvents.Wallet
{
    public record WalletStateChangedEvent(
        Guid WalletId,
        WalletState PreviousState,
        WalletState NewState) : WalletEventBase(WalletId)
    {
        public WalletState PreviousState { get; init; } = PreviousState;
        public WalletState NewState { get; init; } = NewState;
    }
}
