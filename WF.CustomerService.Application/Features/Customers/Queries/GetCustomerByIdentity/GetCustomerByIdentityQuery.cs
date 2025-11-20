using MediatR;
using WF.Shared.Contracts.Dtos;

namespace WF.CustomerService.Application.Features.Customers.Queries.GetCustomerByIdentity;

public class GetCustomerByIdentityQuery : IRequest<CustomerLookupDto>
{
    public string IdentityId { get; set; } = string.Empty;
}
