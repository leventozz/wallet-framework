using MediatR;
using WF.Shared.Contracts.Result;

namespace WF.WalletService.Application.Features.Wallets.Commands.CreateWalletForCustomer
{
    public record class CreateWalletForCustomerCommand : IRequest<Result<Guid>>
    {
        public Guid CustomerId { get; init; }
    }
}

