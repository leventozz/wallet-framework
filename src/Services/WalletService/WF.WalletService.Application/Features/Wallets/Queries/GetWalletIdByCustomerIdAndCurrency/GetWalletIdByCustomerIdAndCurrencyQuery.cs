using MediatR;
using WF.Shared.Contracts.Result;

namespace WF.WalletService.Application.Features.Wallets.Queries.GetWalletIdByCustomerIdAndCurrency;

public record GetWalletIdByCustomerIdAndCurrencyQuery : IRequest<Result<Guid>>
{
    public Guid CustomerId { get; init; }
    public string Currency { get; init; } = string.Empty;
}

