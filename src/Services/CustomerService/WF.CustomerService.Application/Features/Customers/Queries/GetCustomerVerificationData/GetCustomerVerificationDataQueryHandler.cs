using MediatR;
using WF.CustomerService.Application.Abstractions;
using WF.Shared.Contracts.Dtos;
using WF.Shared.Contracts.Result;

namespace WF.CustomerService.Application.Features.Customers.Queries.GetCustomerVerificationData;

public class GetCustomerVerificationDataQueryHandler(ICustomerQueryService _customerQueryService) : IRequestHandler<GetCustomerVerificationDataQuery, Result<CustomerVerificationDto>>
{
    public async Task<Result<CustomerVerificationDto>> Handle(GetCustomerVerificationDataQuery request, CancellationToken cancellationToken)
    {
        var verificationData = await _customerQueryService.GetVerificationDataByIdAsync(request.Id, cancellationToken);

        return verificationData.EnsureExists("Customer", request.Id);
    }
}

