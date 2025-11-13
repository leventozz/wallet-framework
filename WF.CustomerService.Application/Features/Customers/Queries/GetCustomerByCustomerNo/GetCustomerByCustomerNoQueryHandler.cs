using MediatR;
using WF.CustomerService.Application.Abstractions;
using WF.CustomerService.Application.Dtos;
using WF.CustomerService.Domain.Entities;
using WF.Shared.Abstractions.Exceptions;

namespace WF.CustomerService.Application.Features.Customers.Queries.GetCustomerByCustomerNo
{
    public class GetCustomerByCustomerNoQueryHandler(ICustomerQueryService _customerQueryService) : IRequestHandler<GetCustomerByCustomerNoQuery, CustomerDto>
    {
        public async Task<CustomerDto> Handle(GetCustomerByCustomerNoQuery request, CancellationToken cancellationToken)
        {
            var customer = await _customerQueryService.GetCustomerDtoByCustomerNoAsync(request.CustomerNumber, cancellationToken);

            if (customer is null)
            {
                throw new NotFoundException(nameof(Customer), request.CustomerNumber);
            }

            return customer;
        }
    }
}

