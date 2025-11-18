using MediatR;
using WF.Shared.Contracts.Dtos;
using WF.WalletService.Application.Abstractions;

namespace WF.WalletService.Application.Features.Wallets.Queries.LookupByCustomerIds;

public class LookupByCustomerIdsQueryHandler(IWalletQueryService _walletQueryService)
    : IRequestHandler<LookupByCustomerIdsQuery, List<WalletLookupDto>>
{
    public async Task<List<WalletLookupDto>> Handle(LookupByCustomerIdsQuery request, CancellationToken cancellationToken)
    {
        var results = await _walletQueryService.LookupByCustomerIdsAsync(
            request.CustomerIds,
            request.Currency,
            cancellationToken);
        return results;
    }
}

