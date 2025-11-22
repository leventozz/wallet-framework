using MediatR;
using WF.Shared.Contracts.Dtos;
using WF.Shared.Contracts.Result;

namespace WF.CustomerService.Application.Features.Customers.Queries.GetCustomerById
{
    public record GetCustomerByIdQuery : IRequest<Result<CustomerDto>>
    {
        public Guid Id { get; set; }
    }
}
