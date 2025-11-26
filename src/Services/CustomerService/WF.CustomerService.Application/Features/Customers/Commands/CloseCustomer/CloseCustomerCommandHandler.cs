using MediatR;
using Microsoft.Extensions.Logging;
using WF.CustomerService.Domain.Abstractions;
using WF.Shared.Contracts.Abstractions;
using WF.Shared.Contracts.Result;

namespace WF.CustomerService.Application.Features.Customers.Commands.CloseCustomer;

public class CloseCustomerCommandHandler(
    ICustomerRepository _customerRepository,
    IUnitOfWork _unitOfWork,
    ILogger<CloseCustomerCommandHandler> _logger)
    : IRequestHandler<CloseCustomerCommand, Result>
{
    public async Task<Result> Handle(CloseCustomerCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Closing customer with ID {CustomerId}", request.CustomerId);

        var customer = await _customerRepository.GetCustomerByIdAsync(request.CustomerId);

        if (customer is null)
        {
            _logger.LogWarning("Customer with ID {CustomerId} not found", request.CustomerId);
            return Result.Failure(Error.NotFound("Customer", request.CustomerId));
        }

        var result = customer.SetActive(false);

        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to close customer {CustomerId}: {Error}", request.CustomerId, result.Error.Message);
            return result;
        }

        await _customerRepository.UpdateCustomerAsync(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Customer with ID {CustomerId} closed successfully", request.CustomerId);

        return Result.Success();
    }
}
