using MediatR;
using WF.WalletService.Application.Abstractions;
using WF.WalletService.Domain.Entities;
using WF.WalletService.Domain.Enums;
using WF.WalletService.Domain.Repositories;

namespace WF.WalletService.Application.Features.Wallets.Commands.CreateWalletForCustomer
{
    public class CreateWalletForCustomerCommandHandler : IRequestHandler<CreateWalletForCustomerCommand, Guid>
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IUnitOfWork _unitOfWork;
        private const int MaxRetryAttempts = 5;

        public CreateWalletForCustomerCommandHandler(
            IWalletRepository walletRepository,
            IUnitOfWork unitOfWork)
        {
            _walletRepository = walletRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(CreateWalletForCustomerCommand request, CancellationToken cancellationToken)
        {
            string walletNumber = string.Empty;
            bool isUnique = false;
            int attemptCount = 0;

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

            var wallet = new Wallet(request.CustomerId, walletNumber, Currency.TRY);
            await _walletRepository.AddWalletAsync(wallet, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return wallet.Id;
        }
    }
}

