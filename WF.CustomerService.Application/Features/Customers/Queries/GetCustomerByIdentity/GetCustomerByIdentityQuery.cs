using MediatR;
using WF.Shared.Contracts.Dtos;
using WF.Shared.Contracts.Result;

namespace WF.CustomerService.Application.Features.Customers.Queries.GetCustomerByIdentity;

public class GetCustomerByIdentityQuery : IRequest<Result<CustomerLookupDto>>
{
    public string IdentityId { get; set; } = string.Empty;
}
