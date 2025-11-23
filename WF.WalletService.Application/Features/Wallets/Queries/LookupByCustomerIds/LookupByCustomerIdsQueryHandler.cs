using MediatR;
using WF.Shared.Contracts.Dtos;
using WF.Shared.Contracts.Result;
using WF.WalletService.Application.Abstractions;

namespace WF.WalletService.Application.Features.Wallets.Queries.LookupByCustomerIds;

public class LookupByCustomerIdsQueryHandler(IWalletQueryService _walletQueryService)
    : IRequestHandler<LookupByCustomerIdsQuery, Result<List<WalletLookupDto>>>
{
    public async Task<Result<List<WalletLookupDto>>> Handle(LookupByCustomerIdsQuery request, CancellationToken cancellationToken)
    {
        var results = await _walletQueryService.LookupByCustomerIdsAsync(
            request.CustomerIds,
            request.Currency,
            cancellationToken);
        
        if (!results.Any())
        {
            return Result<List<WalletLookupDto>>.Failure(Error.NotFound("Wallets", string.Join(", ", request.CustomerIds)));
        }
        
        return Result<List<WalletLookupDto>>.Success(results);
    }
}

