namespace WF.Shared.Contracts.IntegrationEvents.Transaction
{
    public record WalletCreditedEvent
    {
        public Guid CorrelationId { get; init; }
        public Guid WalletId { get; init; }
        public decimal Amount { get; init; }
    }
}

