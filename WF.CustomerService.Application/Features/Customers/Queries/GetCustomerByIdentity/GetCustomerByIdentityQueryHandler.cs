using MediatR;
using WF.CustomerService.Application.Abstractions;
using WF.CustomerService.Domain.Entities;
using WF.CustomerService.Domain.Exceptions;
using WF.Shared.Contracts.Dtos;

namespace WF.CustomerService.Application.Features.Customers.Queries.GetCustomerByIdentity;

public class GetCustomerByIdentityQueryHandler(ICustomerQueryService _customerQueryService) : IRequestHandler<GetCustomerByIdentityQuery, CustomerLookupDto>
{
    public async Task<CustomerLookupDto> Handle(GetCustomerByIdentityQuery request, CancellationToken cancellationToken)
    {
        var customer = await _customerQueryService.GetCustomerByIdentityAsync(request.IdentityId, cancellationToken);

        if (customer is null)
        {
            throw new NotFoundException(nameof(Customer), request.IdentityId);
        }

        return customer;
    }
}
