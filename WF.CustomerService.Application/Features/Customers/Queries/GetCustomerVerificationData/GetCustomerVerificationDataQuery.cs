using MediatR;
using WF.Shared.Contracts.Dtos;

namespace WF.CustomerService.Application.Features.Customers.Queries.GetCustomerVerificationData;

public record GetCustomerVerificationDataQuery : IRequest<CustomerVerificationDto>
{
    public Guid Id { get; set; }
}

