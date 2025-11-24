namespace WF.Shared.Contracts.Commands.Wallet
{
    public record RefundSenderWalletCommandContract
    {
        public Guid CorrelationId { get; init; }
        public Guid OwnerCustomerId { get; init; }
        public decimal Amount { get; init; }
        public string TransactionId { get; init; } = string.Empty;
    }
}

