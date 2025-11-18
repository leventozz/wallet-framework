using Microsoft.EntityFrameworkCore;
using WF.TransactionService.Domain.Abstractions;
using WF.TransactionService.Domain.Entities;
using WF.TransactionService.Infrastructure.Data;

namespace WF.TransactionService.Infrastructure.Repositories
{
    public class TransferRequestRepository(TransactionDbContext _context) : ITransferRequestRepository
    {
        public async Task AddAsync(Transaction request, CancellationToken cancellationToken)
        {
            await _context.Transactions.AddAsync(request, cancellationToken);
        }

        public async Task<Transaction?> GetByCorrelationIdAsync(Guid correlationId, CancellationToken cancellationToken)
        {
            return await _context.Transactions
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.CorrelationId == correlationId, cancellationToken);
        }

        public void Update(Transaction request)
        {
            _context.Transactions.Update(request);
        }
    }
}

