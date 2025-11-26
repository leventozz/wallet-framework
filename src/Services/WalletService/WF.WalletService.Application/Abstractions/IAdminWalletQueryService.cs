using WF.Shared.Contracts.Result;
using WF.WalletService.Application.Dtos;
using WF.WalletService.Application.Dtos.Filters;

namespace WF.WalletService.Application.Abstractions;

public interface IAdminWalletQueryService
{
    Task<PagedResult<AdminWalletListDto>> GetWalletsAsync(WalletListFilter filter, CancellationToken cancellationToken);
}
