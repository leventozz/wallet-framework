using MediatR;
using WF.CustomerService.Application.Abstractions;
using WF.Shared.Contracts.Result;

namespace WF.CustomerService.Application.Features.Customers.Queries.GetAllCustomersWithWallets;

public class GetAllCustomersWithWalletsQueryHandler(IAdminCustomerQueryService _adminQueryService) 
    : IRequestHandler<GetAllCustomersWithWalletsQuery, Result<PagedResult<AdminCustomerListDto>>>
{
    public async Task<Result<PagedResult<AdminCustomerListDto>>> Handle(
        GetAllCustomersWithWalletsQuery request, 
        CancellationToken cancellationToken)
    {
        var pagedResult = await _adminQueryService.GetAllCustomersWithWalletsAsync(
            request.PageNumber, 
            request.PageSize, 
            cancellationToken);

        return Result<PagedResult<AdminCustomerListDto>>.Success(pagedResult);
    }
}
