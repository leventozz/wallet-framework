using MediatR;
using WF.Shared.Contracts.Dtos;
using WF.Shared.Contracts.Result;

namespace WF.CustomerService.Application.Features.Customers.Queries.LookupByCustomerNumbers;

public record LookupByCustomerNumbersQuery : IRequest<Result<List<CustomerLookupDto>>>
{
    public List<string> CustomerNumbers { get; init; } = new();
}

