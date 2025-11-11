using MediatR;

namespace WF.WalletService.Application.Features.Wallets.Commands.CreateWalletForCustomer
{
    public record class CreateWalletForCustomerCommand : IRequest<Guid>
    {
        public Guid CustomerId { get; init; }
    }
}

