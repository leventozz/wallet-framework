namespace WF.TransactionService.Application.Dtos.Filters;

public record TransactionListFilter
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
