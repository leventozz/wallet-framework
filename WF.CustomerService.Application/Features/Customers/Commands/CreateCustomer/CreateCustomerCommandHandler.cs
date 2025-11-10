using MediatR;
using WF.CustomerService.Application.Abstractions;
using WF.CustomerService.Domain.Entities;
using WF.CustomerService.Domain.Repositories;
using WF.Shared.Contracts.IntegrationEvents;

namespace WF.CustomerService.Application.Features.Customers.Commands.CreateCustomer
{
    public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Guid>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IIntegrationEventPublisher _integrationEventPublisher;
        private readonly IUnitOfWork _unitOfWork;
        private const int MaxRetryAttempts = 5;

        public CreateCustomerCommandHandler(ICustomerRepository customerRepository, IIntegrationEventPublisher integrationEventPublisher, IUnitOfWork unitOfWork)
        {
            _customerRepository = customerRepository;
            _integrationEventPublisher = integrationEventPublisher;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            string customerNumber = string.Empty;
            bool isUnique = false;
            int attemptCount = 0;

            while (!isUnique && attemptCount < MaxRetryAttempts)
            {
                customerNumber = Random.Shared.Next(10000000, 99999999).ToString();
                isUnique = await _customerRepository.IsCustomerNumberUniqueAsync(customerNumber, cancellationToken);
                attemptCount++;
            }

            if (!isUnique)
            {
                throw new InvalidOperationException(
                    $"Unable to generate a unique customer number after {MaxRetryAttempts} attempts. This may indicate that the system is approaching capacity.");
            }

            var customer = new Customer(request.FirstName, request.LastName, request.Email, customerNumber, request.PhoneNumber);
            await _customerRepository.AddCustomerAsync(customer);
            await _integrationEventPublisher.PublishAsync(new CustomerCreatedEvent() { CustomerId = customer.Id },cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return customer.Id;
        }
    }
}
