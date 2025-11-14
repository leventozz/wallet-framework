namespace WF.TransactionService.Application.Dtos
{
    public record TransactionDto
    {
        public Guid CorrelationId { get; init; }
        public string CurrentState { get; init; } = string.Empty;
        public Guid SenderCustomerId { get; init; }
        public string SenderCustomerNumber { get; init; } = string.Empty;
        public Guid ReceiverCustomerId { get; init; }
        public string ReceiverCustomerNumber { get; init; } = string.Empty;
        public Guid SenderWalletId { get; init; }
        public string SenderWalletNumber { get; init; } = string.Empty;
        public Guid ReceiverWalletId { get; init; }
        public string ReceiverWalletNumber { get; init; } = string.Empty;
        public decimal Amount { get; init; }
        public string Currency { get; init; } = string.Empty;
        public DateTime CreatedAtUtc { get; init; }
        public DateTime? CompletedAtUtc { get; init; }
        public string? FailureReason { get; init; }
    }
}

