using MediatR;
using WF.CustomerService.Application.Dtos;
using WF.CustomerService.Domain.Entities;
using WF.CustomerService.Domain.Repositories;
using WF.Shared.Abstractions.Exceptions;

namespace WF.CustomerService.Application.Features.Customers.Queries.GetCustomerById
{
    public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDto>
    {
        private readonly ICustomerRepository _customerRepository;

        public GetCustomerByIdQueryHandler(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<CustomerDto> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.GetCustomerByIdAsync(request.Id);

            if (customer is null)
            {
                throw new NotFoundException(nameof(Customer), request.Id);
            }

            return new CustomerDto
            {
                CustomerNumber = customer.CustomerNumber,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber,
                KycStatus = customer.KycStatus,
                CreatedAtUtc = customer.CreatedAtUtc
            };
        }
    }
}
