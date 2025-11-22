using MediatR;
using WF.CustomerService.Application.Abstractions;
using WF.Shared.Contracts.Result;

namespace WF.CustomerService.Application.Features.Customers.Queries.GetCustomerIdByCustomerNumber
{
    public class GetCustomerIdByCustomerNumberQueryHandler(ICustomerQueryService _customerQueryService) : IRequestHandler<GetCustomerIdByCustomerNumberQuery, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(GetCustomerIdByCustomerNumberQuery request, CancellationToken cancellationToken)
        {
            var customerId = await _customerQueryService.GetCustomerIdByCustomerNumberAsync(request.CustomerNumber, cancellationToken);
            
            if(!customerId.HasValue)
            {
                return Result<Guid>.Failure(Error.NotFound("Customer", request.CustomerNumber));
            }

            return Result<Guid>.Success(customerId.Value);
        }
    }
}

