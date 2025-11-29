using MediatR;
using Microsoft.Extensions.Logging;
using WF.Shared.Contracts.Abstractions;
using WF.Shared.Contracts.IntegrationEvents.Transaction;
using WF.Shared.Contracts.IntegrationEvents.Wallet;
using WF.WalletService.Domain.Abstractions;
using WF.WalletService.Domain.ValueObjects;

namespace WF.WalletService.Application.Features.Wallets.Commands.CreditWallet
{
    public class CreditWalletCommandHandler(
        IWalletRepository _walletRepository,
        IUnitOfWork _unitOfWork,
        IIntegrationEventPublisher _eventPublisher,
        ILogger<CreditWalletCommandHandler> _logger)
        : IRequestHandler<CreditWalletCommand>
    {
        public async Task Handle(CreditWalletCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Processing CreditWalletCommand for WalletId {WalletId}, Amount {Amount}, CorrelationId {CorrelationId}",
                request.WalletId,
                request.Amount,
                request.CorrelationId);

            var wallet = await _walletRepository.GetWalletByIdAsync(
                request.WalletId,
                cancellationToken);

            if (wallet == null)
            {
                await HandleFailureAsync(
                    request.CorrelationId,
                    $"Wallet not found for WalletId {request.WalletId}",
                    "Wallet not found for WalletId {WalletId}, CorrelationId {CorrelationId}",
                    [request.WalletId, request.CorrelationId],
                    cancellationToken);
                return;
            }

            var depositAmountResult = Money.Create(request.Amount, request.Currency);
            if (depositAmountResult.IsFailure)
            {
                await HandleFailureAsync(
                    request.CorrelationId,
                    depositAmountResult.Error.Message,
                    "Invalid amount for WalletId {WalletId}, CorrelationId {CorrelationId}, Reason {Reason}",
                    [wallet.Id, request.CorrelationId, depositAmountResult.Error.Message],
                    cancellationToken);
                return;
            }

            var depositResult = wallet.Deposit(depositAmountResult.Value);
            if (depositResult.IsFailure)
            {
                await HandleFailureAsync(
                    request.CorrelationId,
                    depositResult.Error.Message,
                    "Failed to credit wallet for WalletId {WalletId}, CorrelationId {CorrelationId}, Reason {Reason}",
                    [wallet.Id, request.CorrelationId, depositResult.Error.Message],
                    cancellationToken);
                return;
            }

            var updateTransactionResult = wallet.UpdateLastTransaction(request.TransactionId);
            if (updateTransactionResult.IsFailure)
            {
                await HandleFailureAsync(
                    request.CorrelationId,
                    updateTransactionResult.Error.Message,
                    "Failed to update transaction info for WalletId {WalletId}, CorrelationId {CorrelationId}, Reason {Reason}",
                    [wallet.Id, request.CorrelationId, updateTransactionResult.Error.Message],
                    cancellationToken);
                return;
            }

            await _walletRepository.UpdateWalletAsync(wallet, cancellationToken);

            if(request.Amount == 666)
                throw new Exception("Simulated exception for testing purposes.");

            var successEvent = new WalletCreditedEvent
            {
                CorrelationId = request.CorrelationId,
                WalletId = wallet.Id,
                Amount = request.Amount
            };

            await _eventPublisher.PublishAsync(successEvent, cancellationToken);

            var balanceUpdatedEvent = new WalletBalanceUpdatedEvent(
                wallet.Id,
                wallet.Balance.Amount,
                DateTime.UtcNow);

            await _eventPublisher.PublishAsync(balanceUpdatedEvent, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Wallet credited successfully. WalletId {WalletId}, Amount {Amount}, CorrelationId {CorrelationId}",
                wallet.Id,
                request.Amount,
                request.CorrelationId);
        }

        private async Task HandleFailureAsync(
            Guid correlationId,
            string reason,
            string logMessage,
            object[] logArgs,
            CancellationToken cancellationToken)
        {
            var failureEvent = new WalletCreditFailedEvent
            {
                CorrelationId = correlationId,
                Reason = reason
            };

            await _eventPublisher.PublishAsync(failureEvent, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogWarning(logMessage, logArgs);
        }
    }
}

