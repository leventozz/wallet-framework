using MediatR;
using Microsoft.Extensions.Logging;
using WF.Shared.Contracts.Abstractions;
using WF.Shared.Contracts.IntegrationEvents.Transaction;
using WF.Shared.Contracts.IntegrationEvents.Wallet;
using WF.WalletService.Domain.Abstractions;

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

            var wallet = await _walletRepository.GetWalletByCustomerIdForUpdateAsync(
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

            try
            {
                wallet.Deposit(request.Amount);
                await _walletRepository.UpdateWalletAsync(wallet, cancellationToken);

                var refundEvent = new SenderRefundedEvent
                {
                    CorrelationId = request.CorrelationId,
                    WalletId = wallet.Id,
                    Amount = request.Amount
                };

                await _eventPublisher.PublishAsync(refundEvent, cancellationToken);

                var balanceUpdatedEvent = new WalletBalanceUpdatedEvent(
                    wallet.Id,
                    wallet.Balance,
                    DateTime.UtcNow);

                await _eventPublisher.PublishAsync(balanceUpdatedEvent, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Wallet refunded successfully. WalletId {WalletId}, Amount {Amount}, CorrelationId {CorrelationId}",
                    wallet.Id,
                    request.Amount,
                    request.CorrelationId);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(
                    ex,
                    "Invalid amount for refund. WalletId {WalletId}, CorrelationId {CorrelationId}, Reason {Reason}",
                    wallet.Id,
                    request.CorrelationId,
                    ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error refunding wallet for WalletId {WalletId}, CorrelationId {CorrelationId}",
                    wallet.Id,
                    request.CorrelationId);
            }
        }
    }
}

