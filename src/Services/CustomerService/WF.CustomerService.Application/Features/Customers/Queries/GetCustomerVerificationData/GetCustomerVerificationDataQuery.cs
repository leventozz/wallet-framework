using MediatR;
using WF.Shared.Contracts.Dtos;
using WF.Shared.Contracts.Result;

namespace WF.CustomerService.Application.Features.Customers.Queries.GetCustomerVerificationData;

public record GetCustomerVerificationDataQuery : IRequest<Result<CustomerVerificationDto>>
{
    public Guid Id { get; set; }
}

