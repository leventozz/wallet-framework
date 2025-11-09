using MediatR;
using WF.CustomerService.Domain.Entities;
using WF.CustomerService.Domain.Repositories;

namespace WF.CustomerService.Application.Features.Customers.Commands.CreateCustomer
{
    public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Guid>
    {
        private readonly ICustomerRepository _customerRepository;

        public CreateCustomerCommandHandler(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<Guid> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = new Customer(request.FirstName, request.LastName, request.Email);
            await _customerRepository.AddCustomerAsync(customer);
            return customer.Id;
        }
    }
}
