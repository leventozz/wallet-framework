namespace WF.Shared.Contracts.IntegrationEvents
{
    public record CustomerCreatedEvent
    {
        public Guid CustomerId { get; init; }
    }
}
