using MediatR;
using WF.Shared.Abstractions;
using WF.Shared.Contracts.IntegrationEvents.Wallet;
using WF.WalletService.Application.Abstractions;
using WF.WalletService.Domain.Entities;
using WF.WalletService.Domain.Enums;
using WF.WalletService.Domain.Repositories;

namespace WF.WalletService.Application.Features.Wallets.Commands.CreateWalletForCustomer
{
    public class CreateWalletForCustomerCommandHandler(
        IWalletRepository _walletRepository, 
        IUnitOfWork _unitOfWork,
        IIntegrationEventPublisher _eventPublisher) 
        : IRequestHandler<CreateWalletForCustomerCommand, Guid>
    {
        private const int MaxRetryAttempts = 5;

        public async Task<Guid> Handle(CreateWalletForCustomerCommand request, CancellationToken cancellationToken)
        {
            string walletNumber = string.Empty;
            bool isUnique = false;
            int attemptCount = 0;

            //for extra security, check is customer is exist 

            while (!isUnique && attemptCount < MaxRetryAttempts)
            {
                walletNumber = Random.Shared.Next(10000000, 99999999).ToString();
                isUnique = await _walletRepository.IsWalletNumberUniqueAsync(walletNumber, cancellationToken);
                attemptCount++;
            }

            if (!isUnique)
            {
                throw new InvalidOperationException(
                    $"Unable to generate a unique wallet number after {MaxRetryAttempts} attempts. This may indicate that the system is approaching capacity.");
            }

            var wallet = new Wallet(request.CustomerId, walletNumber);
            await _walletRepository.AddWalletAsync(wallet, cancellationToken);

            var eventToPublish = new WalletCreatedEvent(
                wallet.Id,
                request.CustomerId,
                wallet.Balance,
                Currency.TRY.ToString() //default currency
            );

            await _eventPublisher.PublishAsync(eventToPublish, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return wallet.Id;
        }
    }
}

