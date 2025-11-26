using MediatR;
using WF.Shared.Contracts.Result;
using WF.TransactionService.Application.Dtos;

namespace WF.TransactionService.Application.Features.Admin.Queries.GetAdminTransactions;

public class GetAdminTransactionsQuery : IRequest<Result<PagedResult<AdminTransactionListDto>>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public Guid? CorrelationId { get; init; }
    public string? TransactionId { get; init; }
    public string? CurrentState { get; init; }
    public string? SenderCustomerNumber { get; init; }
    public string? ReceiverCustomerNumber { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}
