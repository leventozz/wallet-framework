namespace WF.TransactionService.Application.Dtos;

public record AdminTransactionListDto
{
    public Guid CorrelationId { get; init; }
    public string TransactionId { get; init; } = string.Empty;
    public string CurrentState { get; init; } = string.Empty;
    public string SenderCustomerNumber { get; init; } = string.Empty;
    public string ReceiverCustomerNumber { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? CompletedAtUtc { get; init; }
    public string? FailureReason { get; init; }
    public string? ClientIpAddress { get; init; }
}
