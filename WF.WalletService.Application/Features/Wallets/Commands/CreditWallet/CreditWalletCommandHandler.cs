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

            var wallet = await _walletRepository.GetWalletByIdForUpdateAsync(
                request.WalletId,
                cancellationToken);

            if (wallet == null)
            {
                var failureEvent = new WalletCreditFailedEvent
                {
                    CorrelationId = request.CorrelationId,
                    Reason = $"Wallet not found for WalletId {request.WalletId}"
                };

                await _eventPublisher.PublishAsync(failureEvent, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogWarning(
                    "Wallet not found for WalletId {WalletId}, CorrelationId {CorrelationId}",
                    request.WalletId,
                    request.CorrelationId);

                return;
            }

            if (!wallet.IsActive)
            {
                var failureEvent = new WalletCreditFailedEvent
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
                var failureEvent = new WalletCreditFailedEvent
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
                var failureEvent = new WalletCreditFailedEvent
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
                var depositAmount = Money.Create(request.Amount, request.Currency);
                wallet.Deposit(depositAmount);
                await _walletRepository.UpdateWalletAsync(wallet, cancellationToken);

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
            catch (ArgumentException ex)
            {
                var failureEvent = new WalletCreditFailedEvent
                {
                    CorrelationId = request.CorrelationId,
                    Reason = ex.Message
                };

                await _eventPublisher.PublishAsync(failureEvent, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogWarning(
                    "Invalid amount for WalletId {WalletId}, CorrelationId {CorrelationId}, Reason {Reason}",
                    wallet.Id,
                    request.CorrelationId,
                    ex.Message);
            }
            catch (Exception ex)
            {
                var failureEvent = new WalletCreditFailedEvent
                {
                    CorrelationId = request.CorrelationId,
                    Reason = $"An error occurred while crediting wallet: {ex.Message}"
                };

                await _eventPublisher.PublishAsync(failureEvent, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogError(
                    ex,
                    "Error crediting wallet for WalletId {WalletId}, CorrelationId {CorrelationId}",
                    wallet.Id,
                    request.CorrelationId);
            }
        }
    }
}

