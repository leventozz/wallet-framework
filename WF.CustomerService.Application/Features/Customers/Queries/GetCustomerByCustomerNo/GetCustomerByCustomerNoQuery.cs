using MediatR;
using WF.CustomerService.Application.Dtos;

namespace WF.CustomerService.Application.Features.Customers.Queries.GetCustomerByCustomerNo
{
    public record GetCustomerByCustomerNoQuery : IRequest<CustomerDto>
    {
        public string CustomerNumber { get; set; } = string.Empty;
    }
}

