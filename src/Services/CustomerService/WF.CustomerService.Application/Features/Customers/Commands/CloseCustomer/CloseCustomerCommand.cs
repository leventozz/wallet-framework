using MediatR;
using WF.Shared.Contracts.Result;

namespace WF.CustomerService.Application.Features.Customers.Commands.CloseCustomer;

public record CloseCustomerCommand(Guid CustomerId) : IRequest<Result>;
