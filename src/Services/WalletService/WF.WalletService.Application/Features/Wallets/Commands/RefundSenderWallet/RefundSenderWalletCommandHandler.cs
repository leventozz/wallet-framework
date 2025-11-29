using MediatR;
using Microsoft.Extensions.Logging;
using WF.Shared.Contracts.Abstractions;
using WF.Shared.Contracts.IntegrationEvents.Transaction;
using WF.Shared.Contracts.IntegrationEvents.Wallet;
using WF.Shared.Contracts.Result;
using WF.WalletService.Domain.Abstractions;
using WF.WalletService.Domain.ValueObjects;

namespace WF.WalletService.Application.Features.Wallets.Commands.RefundSenderWallet
{
    public class RefundSenderWalletCommandHandler(
        IWalletRepository _walletRepository,
        IUnitOfWork _unitOfWork,
        IIntegrationEventPublisher _eventPublisher,
        ILogger<RefundSenderWalletCommandHandler> _logger)
        : IRequestHandler<RefundSenderWalletCommand>
    {
        public async Task Handle(RefundSenderWalletCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Processing RefundSenderWalletCommand for CustomerId {CustomerId}, Amount {Amount}, CorrelationId {CorrelationId}",
                request.OwnerCustomerId,
                request.Amount,
                request.CorrelationId);

            var wallet = await _walletRepository.GetWalletByCustomerIdAsync(
                request.OwnerCustomerId,
                cancellationToken);

            if (wallet == null)
            {
                _logger.LogError(
                    "Wallet not found for CustomerId {CustomerId}, CorrelationId {CorrelationId}. Refund cannot be processed.",
                    request.OwnerCustomerId,
                    request.CorrelationId);

                return;
            }

            if (wallet.IsDeleted)
            {
                _logger.LogWarning(
                    "Wallet {WalletId} is deleted, cannot process refund. CorrelationId {CorrelationId}",
                    wallet.Id,
                    request.CorrelationId);

                return;
            }

            var refundAmountResult = Money.Create(request.Amount, wallet.Balance.Currency);
            if (refundAmountResult.IsFailure)
            {
                _logger.LogError(
                    "Invalid amount for refund. WalletId {WalletId}, CorrelationId {CorrelationId}, Reason {Reason}",
                    wallet.Id,
                    request.CorrelationId,
                    refundAmountResult.Error.Message);

                return;
            }

            var depositResult = wallet.Deposit(refundAmountResult.Value);
            if (depositResult.IsFailure)
            {
                _logger.LogError(
                    "Failed to refund wallet for WalletId {WalletId}, CorrelationId {CorrelationId}, Reason {Reason}",
                    wallet.Id,
                    request.CorrelationId,
                    depositResult.Error.Message);

                return;
            }

            var updateTransactionResult = wallet.UpdateLastTransaction(request.TransactionId);
            if (updateTransactionResult.IsFailure)
            {
                _logger.LogError(
                    "Failed to update transaction info for refund. WalletId {WalletId}, CorrelationId {CorrelationId}, Reason {Reason}",
                    wallet.Id,
                    request.CorrelationId,
                    updateTransactionResult.Error.Message);

                return;
            }



            var refundEvent = new SenderRefundedEvent
            {
                CorrelationId = request.CorrelationId,
                WalletId = wallet.Id,
                Amount = request.Amount
            };

            await _eventPublisher.PublishAsync(refundEvent, cancellationToken);

            var balanceUpdatedEvent = new WalletBalanceUpdatedEvent(
                wallet.Id,
                wallet.Balance.Amount,
                DateTime.UtcNow);

            await _eventPublisher.PublishAsync(balanceUpdatedEvent, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Wallet refunded successfully. WalletId {WalletId}, Amount {Amount}, CorrelationId {CorrelationId}",
                wallet.Id,
                request.Amount,
                request.CorrelationId);
        }
    }
}

