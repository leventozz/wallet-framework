using MediatR;
using WF.Shared.Contracts.Result;
using WF.TransactionService.Application.Contracts;
using WF.TransactionService.Application.Dtos;
using WF.TransactionService.Application.Dtos.Filters;

namespace WF.TransactionService.Application.Features.Admin.Queries.GetAdminTransactions;

public class GetAdminTransactionsQueryHandler(IAdminTransactionQueryService _queryService) 
    : IRequestHandler<GetAdminTransactionsQuery, Result<PagedResult<AdminTransactionListDto>>>
{
    public async Task<Result<PagedResult<AdminTransactionListDto>>> Handle(GetAdminTransactionsQuery request, CancellationToken cancellationToken)
    {
        var filter = new TransactionListFilter
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            CorrelationId = request.CorrelationId,
            TransactionId = request.TransactionId,
            CurrentState = request.CurrentState,
            SenderCustomerNumber = request.SenderCustomerNumber,
            ReceiverCustomerNumber = request.ReceiverCustomerNumber,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };

        var result = await _queryService.GetTransactionsAsync(filter, cancellationToken);

        return Result<PagedResult<AdminTransactionListDto>>.Success(result);
    }
}
