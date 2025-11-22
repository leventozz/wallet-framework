using MediatR;
using WF.Shared.Contracts.Result;

namespace WF.CustomerService.Application.Features.Customers.Queries.GetCustomerIdByCustomerNumber
{
    public record GetCustomerIdByCustomerNumberQuery : IRequest<Result<Guid>>
    {
        public string CustomerNumber { get; set; } = string.Empty;
    }
}

