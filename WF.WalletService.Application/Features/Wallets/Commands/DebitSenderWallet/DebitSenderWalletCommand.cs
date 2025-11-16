using MediatR;

namespace WF.WalletService.Application.Features.Wallets.Commands.DebitSenderWallet
{
    public record DebitSenderWalletCommand : IRequest
    {
        public Guid CorrelationId { get; init; }
        public Guid OwnerCustomerId { get; init; }
        public decimal Amount { get; init; }
    }
}

