using MediatR;
using WF.CustomerService.Application.Abstractions;
using WF.Shared.Contracts.Dtos;
using WF.Shared.Contracts.Result;

namespace WF.CustomerService.Application.Features.Customers.Queries.GetCustomerByCustomerNo
{
    public class GetCustomerByCustomerNoQueryHandler(ICustomerQueryService _customerQueryService) : IRequestHandler<GetCustomerByCustomerNoQuery, Result<CustomerDto>>
    {
        public async Task<Result<CustomerDto>> Handle(GetCustomerByCustomerNoQuery request, CancellationToken cancellationToken)
        {
            var customer = await _customerQueryService.GetCustomerDtoByCustomerNoAsync(request.CustomerNumber, cancellationToken);

            return customer.EnsureExists("Customer", request.CustomerNumber);
        }
    }
}

