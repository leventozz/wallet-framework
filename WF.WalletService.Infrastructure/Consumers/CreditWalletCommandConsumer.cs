using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using WF.Shared.Contracts.Commands.Wallet;
using WF.WalletService.Application.Features.Wallets.Commands.CreditWallet;

namespace WF.WalletService.Infrastructure.Consumers
{
    public class CreditWalletCommandConsumer(
        IMediator _mediator,
        ILogger<CreditWalletCommandConsumer> _logger)
        : IConsumer<CreditWalletCommand>
    {
        public async Task Consume(ConsumeContext<CreditWalletCommand> context)
        {
            var command = context.Message;

            _logger.LogInformation(
                "CreditWalletCommand received for WalletId {WalletId}, Amount {Amount}, CorrelationId {CorrelationId}",
                command.WalletId,
                command.Amount,
                command.CorrelationId);

            var handlerCommand = new CreditWalletCommand
            {
                CorrelationId = command.CorrelationId,
                WalletId = command.WalletId,
                Amount = command.Amount,
                Currency = command.Currency
            };

            await _mediator.Send(handlerCommand, context.CancellationToken);

            _logger.LogInformation(
                "CreditWalletCommand processed successfully for WalletId {WalletId}, CorrelationId {CorrelationId}",
                command.WalletId,
                command.CorrelationId);
        }
    }
}

