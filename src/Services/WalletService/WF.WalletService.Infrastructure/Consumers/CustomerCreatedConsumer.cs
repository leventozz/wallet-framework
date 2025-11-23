using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using WF.Shared.Contracts.IntegrationEvents.Customer;
using WF.WalletService.Application.Features.Wallets.Commands.CreateWalletForCustomer;

namespace WF.WalletService.Infrastructure.Consumers
{
    public class CustomerCreatedConsumer(IMediator _mediator, ILogger<CustomerCreatedConsumer> _logger) : IConsumer<CustomerCreatedEvent>
    {
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

            var result = await _mediator.Send(command, context.CancellationToken);

            if (result.IsFailure)
            {
                _logger.LogError(
                    "Failed to create wallet for CustomerId {CustomerId}. Error: {ErrorCode} - {ErrorMessage}",
                    customerId,
                    result.Error.Code,
                    result.Error.Message);
                return;
            }

            _logger.LogInformation(
                "Wallet created successfully for CustomerId {CustomerId}",
                customerId);
        }
    }
}

