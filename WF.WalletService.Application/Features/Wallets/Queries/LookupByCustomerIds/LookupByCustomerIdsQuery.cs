using MediatR;
using WF.Shared.Contracts.Dtos;

namespace WF.WalletService.Application.Features.Wallets.Queries.LookupByCustomerIds;

public record LookupByCustomerIdsQuery : IRequest<List<WalletLookupDto>>
{
    public List<Guid> CustomerIds { get; init; } = new();
    public string Currency { get; init; } = string.Empty;
}

