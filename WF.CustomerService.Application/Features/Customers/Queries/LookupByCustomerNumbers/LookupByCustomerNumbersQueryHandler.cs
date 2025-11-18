using MediatR;
using WF.CustomerService.Application.Abstractions;
using WF.Shared.Contracts.Dtos;

namespace WF.CustomerService.Application.Features.Customers.Queries.LookupByCustomerNumbers;

public class LookupByCustomerNumbersQueryHandler(ICustomerQueryService _customerQueryService) 
    : IRequestHandler<LookupByCustomerNumbersQuery, List<CustomerLookupDto>>
{
    public async Task<List<CustomerLookupDto>> Handle(LookupByCustomerNumbersQuery request, CancellationToken cancellationToken)
    {
        var results = await _customerQueryService.LookupByCustomerNumbersAsync(request.CustomerNumbers, cancellationToken);
        return results;
    }
}

