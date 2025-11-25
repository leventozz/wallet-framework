using WF.CustomerService.Application.Features.Customers.Queries.GetAllCustomersWithWallets;
using WF.Shared.Contracts.Result;

namespace WF.CustomerService.Application.Abstractions;

public interface IAdminCustomerQueryService
{
    Task<PagedResult<AdminCustomerListDto>> GetAllCustomersWithWalletsAsync(
        int pageNumber, 
        int pageSize, 
        CancellationToken cancellationToken);
}
