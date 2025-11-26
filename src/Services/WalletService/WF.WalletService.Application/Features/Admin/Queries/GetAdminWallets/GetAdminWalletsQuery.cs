using MediatR;
using WF.Shared.Contracts.Result;
using WF.WalletService.Application.Dtos;

namespace WF.WalletService.Application.Features.Admin.Queries.GetAdminWallets;

public class GetAdminWalletsQuery : IRequest<Result<PagedResult<AdminWalletListDto>>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? WalletNumber { get; init; }
    public string? Currency { get; init; }
    public bool? IsActive { get; init; }
    public bool? IsFrozen { get; init; }
    public bool? IsClosed { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}
