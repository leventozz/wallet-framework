namespace WF.Shared.Contracts.Commands.Fraud
{
    public record CheckFraudCommand
    {
        public Guid CorrelationId { get; init; }
        public Guid SenderCustomerId { get; init; }
        public Guid ReceiverCustomerId { get; init; }
        public decimal Amount { get; init; }
        public string Currency { get; init; } = string.Empty;
    }
}

