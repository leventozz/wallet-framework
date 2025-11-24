using MediatR;

namespace WF.WalletService.Application.Features.Wallets.Commands.CreditWallet
{
    public record CreditWalletCommand : IRequest
    {
        public Guid CorrelationId { get; init; }
        public Guid WalletId { get; init; }
        public decimal Amount { get; init; }
        public string TransactionId { get; init; } = string.Empty;
        public string Currency { get; init; } = string.Empty;
    }
}

