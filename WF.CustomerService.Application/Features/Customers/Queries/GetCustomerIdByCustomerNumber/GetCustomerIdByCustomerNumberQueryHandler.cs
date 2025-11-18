using MediatR;
using WF.CustomerService.Application.Abstractions;

namespace WF.CustomerService.Application.Features.Customers.Queries.GetCustomerIdByCustomerNumber
{
    public class GetCustomerIdByCustomerNumberQueryHandler(ICustomerQueryService _customerQueryService) : IRequestHandler<GetCustomerIdByCustomerNumberQuery, Guid?>
    {
        public async Task<Guid?> Handle(GetCustomerIdByCustomerNumberQuery request, CancellationToken cancellationToken)
        {
            var customerId = await _customerQueryService.GetCustomerIdByCustomerNumberAsync(request.CustomerNumber, cancellationToken);
            return customerId;
        }
    }
}

