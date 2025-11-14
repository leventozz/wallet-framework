namespace WF.Shared.Contracts.Commands.Wallet
{
    public record CreditWalletCommand
    {
        public Guid CorrelationId { get; init; }
        public Guid WalletId { get; init; }
        public decimal Amount { get; init; }
        public string Currency { get; init; } = string.Empty;
    }
}

