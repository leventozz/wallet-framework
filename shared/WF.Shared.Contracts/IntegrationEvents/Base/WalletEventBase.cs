namespace WF.Shared.Contracts.IntegrationEvents.Base
{
    public record WalletEventBase(Guid WalletId)
    {
        public Guid WalletId { get; set; } = WalletId;
    }
}
