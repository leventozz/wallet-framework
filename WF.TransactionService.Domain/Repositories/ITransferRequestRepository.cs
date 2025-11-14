using WF.TransactionService.Domain.Entities;

namespace WF.TransactionService.Domain.Repositories
{
    public interface ITransferRequestRepository
    {
        Task AddAsync(TransferRequest request, CancellationToken cancellationToken);
        Task<TransferRequest?> GetByCorrelationIdAsync(Guid correlationId, CancellationToken cancellationToken);
        void Update(TransferRequest request);
    }
}
