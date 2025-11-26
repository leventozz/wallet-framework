using MediatR;
using WF.Shared.Contracts.Result;
using WF.WalletService.Application.Abstractions;
using WF.WalletService.Application.Dtos;
using WF.WalletService.Application.Dtos.Filters;

namespace WF.WalletService.Application.Features.Admin.Queries.GetAdminWallets;

public class GetAdminWalletsQueryHandler(IAdminWalletQueryService _queryService)
    : IRequestHandler<GetAdminWalletsQuery, Result<PagedResult<AdminWalletListDto>>>
{
    public async Task<Result<PagedResult<AdminWalletListDto>>> Handle(GetAdminWalletsQuery request, CancellationToken cancellationToken)
    {
        var filter = new WalletListFilter
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            WalletNumber = request.WalletNumber,
            Currency = request.Currency,
            IsActive = request.IsActive,
            IsFrozen = request.IsFrozen,
            IsClosed = request.IsClosed,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };

        var result = await _queryService.GetWalletsAsync(filter, cancellationToken);

        return Result<PagedResult<AdminWalletListDto>>.Success(result);
    }
}
