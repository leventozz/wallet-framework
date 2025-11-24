using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using WF.Shared.Contracts.Commands.Wallet;
using WF.WalletService.Application.Features.Wallets.Commands.DebitSenderWallet;

namespace WF.WalletService.Infrastructure.Consumers
{
    public class DebitSenderWalletCommandConsumer(
        IMediator _mediator,
        ILogger<DebitSenderWalletCommandConsumer> _logger)
        : IConsumer<DebitSenderWalletCommandContract>
    {
        public async Task Consume(ConsumeContext<DebitSenderWalletCommandContract> context)
        {
            var command = context.Message;

            _logger.LogInformation(
                "DebitSenderWalletCommand received for CustomerId {CustomerId}, Amount {Amount}, CorrelationId {CorrelationId}",
                command.OwnerCustomerId,
                command.Amount,
                command.CorrelationId);

            var handlerCommand = new DebitSenderWalletCommand
            {
                CorrelationId = command.CorrelationId,
                OwnerCustomerId = command.OwnerCustomerId,
                Amount = command.Amount,
                TransactionId = command.TransactionId
            };

            await _mediator.Send(handlerCommand, context.CancellationToken);

            _logger.LogInformation(
                "DebitSenderWalletCommand processed successfully for CustomerId {CustomerId}, CorrelationId {CorrelationId}",
                command.OwnerCustomerId,
                command.CorrelationId);
        }
    }
}

