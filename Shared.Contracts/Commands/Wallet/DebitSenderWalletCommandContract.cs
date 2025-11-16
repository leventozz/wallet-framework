namespace WF.Shared.Contracts.Commands.Wallet
{
    public record DebitSenderWalletCommandContract
    {
        public Guid CorrelationId { get; init; }
        public Guid OwnerCustomerId { get; init; }
        public decimal Amount { get; init; }
    }
}

