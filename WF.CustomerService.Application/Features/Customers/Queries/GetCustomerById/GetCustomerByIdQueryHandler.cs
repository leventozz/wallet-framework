using MediatR;
using WF.CustomerService.Application.Abstractions;
using WF.Shared.Contracts.Dtos;
using WF.Shared.Contracts.Result;

namespace WF.CustomerService.Application.Features.Customers.Queries.GetCustomerById
{
    public class GetCustomerByIdQueryHandler(ICustomerQueryService _customerQueryService) : IRequestHandler<GetCustomerByIdQuery, Result<CustomerDto>>
    {
        public async Task<Result<CustomerDto>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
        {
            var customer = await _customerQueryService.GetCustomerDtoByIdAsync(request.Id, cancellationToken);

            return customer.EnsureExists("Customer", request.Id);
        }
    }
}
