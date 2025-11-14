namespace WF.Shared.Contracts.IntegrationEvents.Transaction
{
    public record FraudCheckApprovedEvent
    {
        public Guid CorrelationId { get; init; }
    }
}

