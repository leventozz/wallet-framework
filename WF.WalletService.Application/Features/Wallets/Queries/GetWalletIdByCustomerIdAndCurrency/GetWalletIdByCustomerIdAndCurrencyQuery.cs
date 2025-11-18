using MediatR;

namespace WF.WalletService.Application.Features.Wallets.Queries.GetWalletIdByCustomerIdAndCurrency;

public record GetWalletIdByCustomerIdAndCurrencyQuery : IRequest<Guid?>
{
    public Guid CustomerId { get; init; }
    public string Currency { get; init; } = string.Empty;
}

