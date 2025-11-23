using MediatR;
using WF.Shared.Contracts.Dtos;
using WF.Shared.Contracts.Result;

namespace WF.CustomerService.Application.Features.Customers.Queries.GetCustomerByCustomerNo
{
    public record GetCustomerByCustomerNoQuery : IRequest<Result<CustomerDto>>
    {
        public string CustomerNumber { get; set; } = string.Empty;
    }
}

