using WF.TransactionService.Application.Dtos;

namespace WF.TransactionService.Application.Contracts
{
    public interface ITransactionReadService
    {
        Task<List<TransactionDto>> GetTransactionsByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken);
    }
}

