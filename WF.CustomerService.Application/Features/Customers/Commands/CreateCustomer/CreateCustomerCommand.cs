using MediatR;

namespace WF.CustomerService.Application.Features.Customers.Commands.CreateCustomer
{
    public record class CreateCustomerCommand : IRequest<Guid>
    {
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
    }
}
