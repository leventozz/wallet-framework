using MassTransit;

namespace WF.TransactionService.Domain.Entities
{
    public class Transaction : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public Guid SenderCustomerId { get; set; }
        public string SenderCustomerNumber { get; set; } = string.Empty;
        public Guid ReceiverCustomerId { get; set; }
        public string ReceiverCustomerNumber { get; set; } = string.Empty;
        public Guid SenderWalletId { get; set; }
        public Guid ReceiverWalletId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? CompletedAtUtc { get; set; }
        public string? FailureReason { get; set; }
    }
}
