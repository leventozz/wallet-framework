using MediatR;
using WF.CustomerService.Application.Abstractions;
using WF.CustomerService.Application.Dtos;
using WF.CustomerService.Domain.Entities;
using WF.Shared.Abstractions.Exceptions;

namespace WF.CustomerService.Application.Features.Customers.Queries.GetCustomerById
{
    public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDto>
    {
        private readonly ICustomerQueryService _customerQueryService;

        public GetCustomerByIdQueryHandler(ICustomerQueryService customerQueryService)
        {
            _customerQueryService = customerQueryService;
        }

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
