using WF.Shared.Contracts.Result;
using WF.TransactionService.Application.Dtos;
using WF.TransactionService.Application.Dtos.Filters;

namespace WF.TransactionService.Application.Contracts;

public interface IAdminTransactionQueryService
{
    Task<PagedResult<AdminTransactionListDto>> GetTransactionsAsync(TransactionListFilter filter, CancellationToken cancellationToken);
}
