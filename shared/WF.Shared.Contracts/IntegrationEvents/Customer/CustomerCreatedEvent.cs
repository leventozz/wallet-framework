namespace WF.Shared.Contracts.IntegrationEvents.Customer
{
    public record CustomerCreatedEvent
    {
        public Guid CustomerId { get; init; }
    }
}
