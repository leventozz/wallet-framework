using MediatR;
using WF.CustomerService.Application.Abstractions;
using WF.CustomerService.Domain.Entities;
using WF.CustomerService.Domain.Exceptions;
using WF.Shared.Contracts.Dtos;

namespace WF.CustomerService.Application.Features.Customers.Queries.GetCustomerVerificationData;

public class GetCustomerVerificationDataQueryHandler(ICustomerQueryService _customerQueryService) : IRequestHandler<GetCustomerVerificationDataQuery, CustomerVerificationDto>
{
    public async Task<CustomerVerificationDto> Handle(GetCustomerVerificationDataQuery request, CancellationToken cancellationToken)
    {
        var verificationData = await _customerQueryService.GetVerificationDataByIdAsync(request.Id, cancellationToken);

        if (verificationData is null)
        {
            throw new NotFoundException(nameof(Customer), request.Id);
        }

        return verificationData;
    }
}

