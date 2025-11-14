using WF.TransactionService.Domain.Entities;

namespace WF.TransactionService.Domain.Repositories
{
    public interface ITransferRequestRepository
    {
        Task AddAsync(Transaction request, CancellationToken cancellationToken);
        Task<Transaction?> GetByCorrelationIdAsync(Guid correlationId, CancellationToken cancellationToken);
        void Update(Transaction request);
    }
}
