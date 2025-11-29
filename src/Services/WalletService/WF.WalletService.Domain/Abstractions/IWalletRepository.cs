using WF.WalletService.Domain.Entities;

namespace WF.WalletService.Domain.Abstractions
{
    public interface IWalletRepository
    {
        Task<Wallet?> GetWalletByIdAsync(Guid walletId, CancellationToken cancellationToken = default);
        Task<Wallet?> GetWalletByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
        Task AddWalletAsync(Wallet wallet, CancellationToken cancellationToken = default);
        Task UpdateWalletAsync(Wallet wallet, CancellationToken cancellationToken = default);
        Task<bool> IsWalletNumberUniqueAsync(string walletNumber, CancellationToken cancellationToken = default);
    }
}

