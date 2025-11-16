using MediatR;
using WF.Shared.Contracts.Dtos;

namespace WF.CustomerService.Application.Features.Customers.Queries.GetCustomerByCustomerNo
{
    public record GetCustomerByCustomerNoQuery : IRequest<CustomerDto>
    {
        public string CustomerNumber { get; set; } = string.Empty;
    }
}

