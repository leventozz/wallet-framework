using MediatR;
using Microsoft.Extensions.Logging;
using WF.Shared.Contracts.Abstractions;
using WF.Shared.Contracts.IntegrationEvents.Transaction;
using WF.Shared.Contracts.IntegrationEvents.Wallet;
using WF.WalletService.Domain.Abstractions;
using WF.WalletService.Domain.ValueObjects;

namespace WF.WalletService.Application.Features.Wallets.Commands.DebitSenderWallet
{
    public class DebitSenderWalletCommandHandler(
        IWalletRepository _walletRepository,
        IUnitOfWork _unitOfWork,
        IIntegrationEventPublisher _eventPublisher,
        ILogger<DebitSenderWalletCommandHandler> _logger)
        : IRequestHandler<DebitSenderWalletCommand>
    {
        public async Task Handle(DebitSenderWalletCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Processing DebitSenderWalletCommand for CustomerId {CustomerId}, Amount {Amount}, CorrelationId {CorrelationId}",
                request.OwnerCustomerId,
                request.Amount,
                request.CorrelationId);

            var wallet = await _walletRepository.GetWalletByCustomerIdAsync(
                request.OwnerCustomerId,
                cancellationToken);

            if (wallet == null)
            {
                await HandleFailureAsync(
                    request.CorrelationId,
                    $"Wallet not found for customer {request.OwnerCustomerId}",
                    "Wallet not found for CustomerId {CustomerId}, CorrelationId {CorrelationId}",
                    [request.OwnerCustomerId, request.CorrelationId],
                    cancellationToken);
                return;
            }

            var withdrawAmountResult = Money.Create(request.Amount, wallet.Balance.Currency);
            if (withdrawAmountResult.IsFailure)
            {
                await HandleFailureAsync(
                    request.CorrelationId,
                    withdrawAmountResult.Error.Message,
                    "Invalid amount for WalletId {WalletId}, CorrelationId {CorrelationId}, Reason {Reason}",
                    [wallet.Id, request.CorrelationId, withdrawAmountResult.Error.Message],
                    cancellationToken);
                return;
            }

            var withdrawResult = wallet.Withdraw(withdrawAmountResult.Value);
            if (withdrawResult.IsFailure)
            {
                await HandleFailureAsync(
                    request.CorrelationId,
                    withdrawResult.Error.Message,
                    "Failed to debit wallet for WalletId {WalletId}, CorrelationId {CorrelationId}, Reason {Reason}",
                    [wallet.Id, request.CorrelationId, withdrawResult.Error.Message],
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

            var successEvent = new WalletDebitedEvent
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
                "Wallet debited successfully. WalletId {WalletId}, Amount {Amount}, CorrelationId {CorrelationId}",
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
            var failureEvent = new WalletDebitFailedEvent
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

