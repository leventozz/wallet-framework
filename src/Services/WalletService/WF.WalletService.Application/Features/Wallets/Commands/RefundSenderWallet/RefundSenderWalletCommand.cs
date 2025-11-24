using MediatR;

namespace WF.WalletService.Application.Features.Wallets.Commands.RefundSenderWallet
{
    public record RefundSenderWalletCommand : IRequest
    {
        public Guid CorrelationId { get; init; }
        public Guid OwnerCustomerId { get; init; }
        public decimal Amount { get; init; }
        public string TransactionId { get; init; } = string.Empty;
    }
}

