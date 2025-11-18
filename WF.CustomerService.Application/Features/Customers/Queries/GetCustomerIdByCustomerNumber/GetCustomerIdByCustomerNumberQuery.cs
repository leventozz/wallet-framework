using MediatR;

namespace WF.CustomerService.Application.Features.Customers.Queries.GetCustomerIdByCustomerNumber
{
    public record GetCustomerIdByCustomerNumberQuery : IRequest<Guid?>
    {
        public string CustomerNumber { get; set; } = string.Empty;
    }
}

