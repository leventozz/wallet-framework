using MediatR;
using Microsoft.Extensions.Logging;
using WF.CustomerService.Application.Abstractions.Identity;
using WF.CustomerService.Domain.Abstractions;
using WF.CustomerService.Domain.Entities;
using WF.CustomerService.Domain.ValueObjects;
using WF.Shared.Contracts.Abstractions;
using WF.Shared.Contracts.IntegrationEvents.Customer;
using WF.Shared.Contracts.Result;

namespace WF.CustomerService.Application.Features.Customers.Commands.CreateCustomer
{
    public class CreateCustomerCommandHandler(
        ICustomerRepository _customerRepository,
        IIdentityService _identityService,
        IIntegrationEventPublisher _integrationEventPublisher,
        IUnitOfWork _unitOfWork,
        ILogger<CreateCustomerCommandHandler> _logger)
        : IRequestHandler<CreateCustomerCommand, Result<Guid>>
    {
        private const int MaxRetryAttempts = 5;

        public async Task<Result<Guid>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
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

            var nameResult = PersonName.Create(request.FirstName, request.LastName);
            if (nameResult.IsFailure)
            {
                _logger.LogWarning("Failed to create person name: {Error}", nameResult.Error.Message);
                return Result<Guid>.Failure(nameResult.Error);
            }
            
            var emailResult = Email.Create(request.Email);
            if (emailResult.IsFailure)
            {
                _logger.LogWarning("Failed to create email: {Error}", emailResult.Error.Message);
                return Result<Guid>.Failure(emailResult.Error);
            }

            var phoneNumberResult = PhoneNumber.Create(request.PhoneNumber);
            if (phoneNumberResult.IsFailure)
            {
                _logger.LogWarning("Failed to create phone number: {Error}", phoneNumberResult.Error.Message);
                return Result<Guid>.Failure(phoneNumberResult.Error);
            }

            var customerResult = Customer.Create(identityId, nameResult.Value, emailResult.Value, customerNumber, phoneNumberResult.Value);
            if (customerResult.IsFailure)
            {
                _logger.LogWarning("Failed to create customer: {Error}",customerResult.Error.Message);
                return Result<Guid>.Failure(customerResult.Error);
            }

            var customer = customerResult.Value;
            await _customerRepository.AddCustomerAsync(customer);
            await _integrationEventPublisher.PublishAsync(new CustomerCreatedEvent() { CustomerId = customer.Id }, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Customer created successfully with ID {CustomerId} and identity ID {IdentityId}",
                customer.Id,
                identityId);

            return Result<Guid>.Success(customer.Id);
        }
    }
}
