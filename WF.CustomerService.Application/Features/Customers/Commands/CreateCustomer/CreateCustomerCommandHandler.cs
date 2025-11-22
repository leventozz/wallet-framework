using MediatR;
using Microsoft.Extensions.Logging;
using WF.CustomerService.Application.Abstractions.Identity;
using WF.CustomerService.Domain.Abstractions;
using WF.CustomerService.Domain.Entities;
using WF.CustomerService.Domain.ValueObjects;
using WF.Shared.Contracts.Abstractions;
using WF.Shared.Contracts.IntegrationEvents.Customer;

namespace WF.CustomerService.Application.Features.Customers.Commands.CreateCustomer
{
    public class CreateCustomerCommandHandler(
        ICustomerRepository _customerRepository,
        IIdentityService _identityService,
        IIntegrationEventPublisher _integrationEventPublisher,
        IUnitOfWork _unitOfWork,
        ILogger<CreateCustomerCommandHandler> _logger)
        : IRequestHandler<CreateCustomerCommand, Guid>
    {
        private const int MaxRetryAttempts = 5;

        public async Task<Guid> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Creating customer with email {Email}",
                request.Email);

            string identityId;
            try
            {
                identityId = await _identityService.RegisterUserAsync(
                    request.Email,
                    request.Password,
                    request.FirstName,
                    request.LastName,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to register user in identity provider for email {Email}",
                    request.Email);
                throw;
            }

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

            var name = new PersonName(request.FirstName, request.LastName);
            var email = new Email(request.Email);
            var phoneNumber = new PhoneNumber(request.PhoneNumber);

            var customer = new Customer(identityId, name, email, customerNumber, phoneNumber);
            await _customerRepository.AddCustomerAsync(customer);
            await _integrationEventPublisher.PublishAsync(new CustomerCreatedEvent() { CustomerId = customer.Id }, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Customer created successfully with ID {CustomerId} and identity ID {IdentityId}",
                customer.Id,
                identityId);

            return customer.Id;
        }
    }
}
