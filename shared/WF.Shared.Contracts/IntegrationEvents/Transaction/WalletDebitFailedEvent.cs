namespace WF.Shared.Contracts.IntegrationEvents.Transaction
{
    public record WalletDebitFailedEvent
    {
        public Guid CorrelationId { get; init; }
        public string Reason { get; init; } = string.Empty;
    }
}

