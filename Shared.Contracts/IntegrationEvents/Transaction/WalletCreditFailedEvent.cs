namespace WF.Shared.Contracts.IntegrationEvents.Transaction
{
    public record WalletCreditFailedEvent
    {
        public Guid CorrelationId { get; init; }
        public string Reason { get; init; } = string.Empty;
    }
}

