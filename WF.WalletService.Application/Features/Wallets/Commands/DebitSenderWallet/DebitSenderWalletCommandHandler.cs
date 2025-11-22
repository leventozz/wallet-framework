using MediatR;
using Microsoft.Extensions.Logging;
using WF.Shared.Contracts.Abstractions;
using WF.Shared.Contracts.IntegrationEvents.Transaction;
using WF.Shared.Contracts.IntegrationEvents.Wallet;
using WF.WalletService.Domain.Abstractions;
using WF.WalletService.Domain.Exceptions;
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

            var wallet = await _walletRepository.GetWalletByCustomerIdForUpdateAsync(
                request.OwnerCustomerId,
                cancellationToken);

            if (wallet == null)
            {
                var failureEvent = new WalletDebitFailedEvent
                {
                    CorrelationId = request.CorrelationId,
                    Reason = $"Wallet not found for customer {request.OwnerCustomerId}"
                };

                await _eventPublisher.PublishAsync(failureEvent, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogWarning(
                    "Wallet not found for CustomerId {CustomerId}, CorrelationId {CorrelationId}",
                    request.OwnerCustomerId,
                    request.CorrelationId);

                return;
            }

            if (!wallet.IsActive)
            {
                var failureEvent = new WalletDebitFailedEvent
                {
                    CorrelationId = request.CorrelationId,
                    Reason = $"Wallet {wallet.Id} is not active"
                };

                await _eventPublisher.PublishAsync(failureEvent, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogWarning(
                    "Wallet {WalletId} is not active, CorrelationId {CorrelationId}",
                    wallet.Id,
                    request.CorrelationId);

                return;
            }

            if (wallet.IsFrozen)
            {
                var failureEvent = new WalletDebitFailedEvent
                {
                    CorrelationId = request.CorrelationId,
                    Reason = $"Wallet {wallet.Id} is frozen"
                };

                await _eventPublisher.PublishAsync(failureEvent, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogWarning(
                    "Wallet {WalletId} is frozen, CorrelationId {CorrelationId}",
                    wallet.Id,
                    request.CorrelationId);

                return;
            }

            if (wallet.IsClosed)
            {
                var failureEvent = new WalletDebitFailedEvent
                {
                    CorrelationId = request.CorrelationId,
                    Reason = $"Wallet {wallet.Id} is closed"
                };

                await _eventPublisher.PublishAsync(failureEvent, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogWarning(
                    "Wallet {WalletId} is closed, CorrelationId {CorrelationId}",
                    wallet.Id,
                    request.CorrelationId);

                return;
            }

            try
            {
                var withdrawAmount = Money.Create(request.Amount, wallet.Balance.Currency);
                wallet.Withdraw(withdrawAmount);
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
            catch (InsufficientBalanceException ex)
            {
                var failureEvent = new WalletDebitFailedEvent
                {
                    CorrelationId = request.CorrelationId,
                    Reason = ex.Message
                };

                await _eventPublisher.PublishAsync(failureEvent, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogWarning(
                    "Insufficient balance for WalletId {WalletId}, CorrelationId {CorrelationId}, Reason {Reason}",
                    wallet.Id,
                    request.CorrelationId,
                    ex.Message);
            }
            catch (Exception ex)
            {
                var failureEvent = new WalletDebitFailedEvent
                {
                    CorrelationId = request.CorrelationId,
                    Reason = $"An error occurred while debiting wallet: {ex.Message}"
                };

                await _eventPublisher.PublishAsync(failureEvent, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogError(
                    ex,
                    "Error debiting wallet for WalletId {WalletId}, CorrelationId {CorrelationId}",
                    wallet.Id,
                    request.CorrelationId);
            }
        }
    }
}

