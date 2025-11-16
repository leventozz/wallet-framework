using MediatR;
using WF.CustomerService.Application.Abstractions;
using WF.CustomerService.Domain.Entities;
using WF.CustomerService.Domain.Exceptions;
using WF.Shared.Contracts.Dtos;

namespace WF.CustomerService.Application.Features.Customers.Queries.GetCustomerById
{
    public class GetCustomerByIdQueryHandler(ICustomerQueryService _customerQueryService) : IRequestHandler<GetCustomerByIdQuery, CustomerDto>
    {
        public async Task<CustomerDto> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
        {
            var customer = await _customerQueryService.GetCustomerDtoByIdAsync(request.Id, cancellationToken);

            if (customer is null)
            {
                throw new NotFoundException(nameof(Customer), request.Id);
            }

            return customer;
        }
    }
}
