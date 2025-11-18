using MediatR;
using WF.Shared.Contracts.Dtos;

namespace WF.CustomerService.Application.Features.Customers.Queries.LookupByCustomerNumbers;

public record LookupByCustomerNumbersQuery : IRequest<List<CustomerLookupDto>>
{
    public List<string> CustomerNumbers { get; init; } = new();
}

