using MediatR;
using WF.Shared.Contracts.Result;

namespace WF.CustomerService.Application.Features.Customers.Queries.GetAllCustomersWithWallets;

public record GetAllCustomersWithWalletsQuery : IRequest<Result<PagedResult<AdminCustomerListDto>>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
