using MassTransit;
using WF.CustomerService.Application.Abstractions;

namespace WF.CustomerService.Infrastructure.EventBus
{
    public class MassTransitEventPublisher : IIntegrationEventPublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public MassTransitEventPublisher(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class
        {
            await _publishEndpoint.Publish(@event, cancellationToken);
        }
    }
}
