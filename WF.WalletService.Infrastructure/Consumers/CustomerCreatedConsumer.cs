using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using WF.Shared.Contracts.IntegrationEvents;
using WF.WalletService.Application.Features.Wallets.Commands.CreateWalletForCustomer;

namespace WF.WalletService.Infrastructure.Consumers
{
    public class CustomerCreatedConsumer : IConsumer<CustomerCreatedEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CustomerCreatedConsumer> _logger;

        public CustomerCreatedConsumer(IMediator mediator, ILogger<CustomerCreatedConsumer> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<CustomerCreatedEvent> context)
        {
            var customerId = context.Message.CustomerId;

            _logger.LogInformation(
                "CustomerCreatedEvent received for CustomerId {CustomerId}",
                customerId);

            var command = new CreateWalletForCustomerCommand
            {
                CustomerId = customerId
            };

            await _mediator.Send(command, context.CancellationToken);

            _logger.LogInformation(
                "Wallet created successfully for CustomerId {CustomerId}",
                customerId);
        }
    }
}

