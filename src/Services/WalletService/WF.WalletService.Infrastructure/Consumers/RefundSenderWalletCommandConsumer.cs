using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using WF.Shared.Contracts.Commands.Wallet;
using WF.WalletService.Application.Features.Wallets.Commands.RefundSenderWallet;

namespace WF.WalletService.Infrastructure.Consumers
{
    public class RefundSenderWalletCommandConsumer(
        IMediator _mediator,
        ILogger<RefundSenderWalletCommandConsumer> _logger)
        : IConsumer<RefundSenderWalletCommandContract>
    {
        public async Task Consume(ConsumeContext<RefundSenderWalletCommandContract> context)
        {
            var command = context.Message;

            _logger.LogInformation(
                "RefundSenderWalletCommand received for CustomerId {CustomerId}, Amount {Amount}, CorrelationId {CorrelationId}",
                command.OwnerCustomerId,
                command.Amount,
                command.CorrelationId);

            var handlerCommand = new RefundSenderWalletCommand
            {
                CorrelationId = command.CorrelationId,
                OwnerCustomerId = command.OwnerCustomerId,
                Amount = command.Amount,
                TransactionId = command.TransactionId
            };

            await _mediator.Send(handlerCommand, context.CancellationToken);

            _logger.LogInformation(
                "RefundSenderWalletCommand processed successfully for CustomerId {CustomerId}, CorrelationId {CorrelationId}",
                command.OwnerCustomerId,
                command.CorrelationId);
        }
    }
}

