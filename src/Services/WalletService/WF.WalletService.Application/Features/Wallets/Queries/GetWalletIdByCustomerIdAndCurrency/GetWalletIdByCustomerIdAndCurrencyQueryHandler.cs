using MediatR;
using WF.Shared.Contracts.Result;
using WF.WalletService.Application.Abstractions;

namespace WF.WalletService.Application.Features.Wallets.Queries.GetWalletIdByCustomerIdAndCurrency;

public class GetWalletIdByCustomerIdAndCurrencyQueryHandler(IWalletQueryService _walletQueryService)
    : IRequestHandler<GetWalletIdByCustomerIdAndCurrencyQuery, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(GetWalletIdByCustomerIdAndCurrencyQuery request, CancellationToken cancellationToken)
    {
        var walletId = await _walletQueryService.GetWalletIdByCustomerIdAndCurrencyAsync(
            request.CustomerId,
            request.Currency,
            cancellationToken);
        
        if (!walletId.HasValue)
        {
            return Result<Guid>.Failure(Error.NotFound("Wallet", $"CustomerId: {request.CustomerId}, Currency: {request.Currency}"));
        }

        return Result<Guid>.Success(walletId.Value);
    }
}

