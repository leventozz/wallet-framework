using MediatR;
using WF.WalletService.Application.Abstractions;

namespace WF.WalletService.Application.Features.Wallets.Queries.GetWalletIdByCustomerIdAndCurrency;

public class GetWalletIdByCustomerIdAndCurrencyQueryHandler(IWalletQueryService _walletQueryService)
    : IRequestHandler<GetWalletIdByCustomerIdAndCurrencyQuery, Guid?>
{
    public async Task<Guid?> Handle(GetWalletIdByCustomerIdAndCurrencyQuery request, CancellationToken cancellationToken)
    {
        return await _walletQueryService.GetWalletIdByCustomerIdAndCurrencyAsync(
            request.CustomerId,
            request.Currency,
            cancellationToken);
    }
}

