using MediatR;
using WF.CustomerService.Application.Dtos;

namespace WF.CustomerService.Application.Features.Customers.Queries.GetCustomerById
{
    public record GetCustomerByIdQuery : IRequest<CustomerDto>
    {
        public Guid Id { get; set; }
    }
}
