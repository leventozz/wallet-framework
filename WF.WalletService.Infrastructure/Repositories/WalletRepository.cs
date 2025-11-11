using Microsoft.EntityFrameworkCore;
using WF.WalletService.Domain.Entities;
using WF.WalletService.Domain.Repositories;
using WF.WalletService.Infrastructure.Data;

namespace WF.WalletService.Infrastructure.Repositories
{
    public class WalletRepository : IWalletRepository
    {
        private readonly WalletDbContext _context;

        public WalletRepository(WalletDbContext context)
        {
            _context = context;
        }

        public async Task AddWalletAsync(Wallet wallet, CancellationToken cancellationToken = default)
        {
            await _context.Wallets.AddAsync(wallet, cancellationToken);
        }

        public async Task<Wallet?> GetWalletByIdAsync(Guid walletId, CancellationToken cancellationToken = default)
        {
            return await _context.Wallets
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == walletId, cancellationToken);
        }

        public async Task<Wallet?> GetWalletByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            return await _context.Wallets
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.CustomerId == customerId, cancellationToken);
        }

        public async Task UpdateWalletAsync(Wallet wallet, CancellationToken cancellationToken = default)
        {
            _context.Wallets.Update(wallet);
            await Task.CompletedTask;
        }

        public async Task<bool> IsWalletNumberUniqueAsync(string walletNumber, CancellationToken cancellationToken = default)
        {
            return !await _context.Wallets
                .AsNoTracking()
                .AnyAsync(w => w.WalletNumber == walletNumber, cancellationToken);
        }
    }
}

