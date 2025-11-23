using MediatR;
using WF.Shared.Contracts.Result;

namespace WF.CustomerService.Application.Features.Customers.Commands.CreateCustomer
{
    public record class CreateCustomerCommand : IRequest<Result<Guid>>
    {
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string PhoneNumber { get; init; } = string.Empty;
    }
}
