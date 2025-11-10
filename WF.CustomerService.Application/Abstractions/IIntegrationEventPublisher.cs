namespace WF.CustomerService.Application.Abstractions
{
    public interface IIntegrationEventPublisher
    {
        Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
            where TEvent : class;
    }
}
