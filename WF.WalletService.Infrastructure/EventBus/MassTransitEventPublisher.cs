using MassTransit;
using WF.WalletService.Application.Abstractions;

namespace WF.WalletService.Infrastructure.EventBus
{
    public class MassTransitEventPublisher(IPublishEndpoint _publishEndpoint) : IIntegrationEventPublisher
    {
        public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class
        {
            await _publishEndpoint.Publish(@event, cancellationToken);
        }
    }
}
