namespace WF.Shared.Contracts.Commands.Wallet
{
    public record RefundSenderWalletCommand
    {
        public Guid CorrelationId { get; init; }
        public Guid OwnerCustomerId { get; init; }
        public decimal Amount { get; init; }
    }
}

