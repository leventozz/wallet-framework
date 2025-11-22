using MediatR;
using WF.CustomerService.Application.Abstractions;
using WF.Shared.Contracts.Dtos;
using WF.Shared.Contracts.Result;

namespace WF.CustomerService.Application.Features.Customers.Queries.LookupByCustomerNumbers;

public class LookupByCustomerNumbersQueryHandler(ICustomerQueryService _customerQueryService) 
    : IRequestHandler<LookupByCustomerNumbersQuery, Result<List<CustomerLookupDto>>>
{
    public async Task<Result<List<CustomerLookupDto>>> Handle(LookupByCustomerNumbersQuery request, CancellationToken cancellationToken)
    {
        var results = await _customerQueryService.LookupByCustomerNumbersAsync(request.CustomerNumbers, cancellationToken);
        
        if (results.Count == 0)
        {
            return Result<List<CustomerLookupDto>>.Failure(Error.NotFound("Customers", string.Join(", ", request.CustomerNumbers)));
        }
        
        return Result<List<CustomerLookupDto>>.Success(results);
    }
}

