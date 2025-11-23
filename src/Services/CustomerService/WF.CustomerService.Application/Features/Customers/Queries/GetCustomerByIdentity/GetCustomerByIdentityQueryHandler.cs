using MediatR;
using WF.CustomerService.Application.Abstractions;
using WF.Shared.Contracts.Dtos;
using WF.Shared.Contracts.Result;

namespace WF.CustomerService.Application.Features.Customers.Queries.GetCustomerByIdentity;

public class GetCustomerByIdentityQueryHandler(ICustomerQueryService _customerQueryService) : IRequestHandler<GetCustomerByIdentityQuery, Result<CustomerLookupDto>>
{
    public async Task<Result<CustomerLookupDto>> Handle(GetCustomerByIdentityQuery request, CancellationToken cancellationToken)
    {
        var customer = await _customerQueryService.GetCustomerByIdentityAsync(request.IdentityId, cancellationToken);

        return customer.EnsureExists("Customer", request.IdentityId);
    }
}
