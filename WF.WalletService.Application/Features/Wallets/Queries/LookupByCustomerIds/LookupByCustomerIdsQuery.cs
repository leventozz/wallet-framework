using MediatR;
using WF.Shared.Contracts.Dtos;
using WF.Shared.Contracts.Result;

namespace WF.WalletService.Application.Features.Wallets.Queries.LookupByCustomerIds;

public record LookupByCustomerIdsQuery : IRequest<Result<List<WalletLookupDto>>>
{
    public List<Guid> CustomerIds { get; init; } = new();
    public string Currency { get; init; } = string.Empty;
}

