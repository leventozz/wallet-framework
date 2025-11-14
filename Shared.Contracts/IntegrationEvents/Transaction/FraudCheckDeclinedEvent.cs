namespace WF.Shared.Contracts.IntegrationEvents.Transaction
{
    public record FraudCheckDeclinedEvent
    {
        public Guid CorrelationId { get; init; }
        public string Reason { get; init; } = string.Empty;
    }
}

