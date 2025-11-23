using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WF.CustomerService.Api.Controllers.Base;
using WF.CustomerService.Application.Features.Customers.Queries.GetCustomerByIdentity;
using WF.CustomerService.Application.Features.Customers.Queries.GetCustomerVerificationData;
using WF.CustomerService.Application.Features.Customers.Queries.LookupByCustomerNumbers;
using WF.Shared.Contracts.Dtos;

namespace WF.CustomerService.Api.Controllers.Internal;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/internal/customers")]
[Authorize(Policy = "ServiceToService")]
[ApiExplorerSettings(IgnoreApi = true)]
public class CustomerInternalController(IMediator _mediator) : BaseController
{
    [HttpGet("by-identity/{identityId}")]
    [ProducesResponseType(typeof(CustomerLookupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomerByIdentity(string identityId, CancellationToken cancellationToken)
    {
        var query = new GetCustomerByIdentityQuery { IdentityId = identityId };
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}/verification-data")]
    [ProducesResponseType(typeof(CustomerVerificationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVerificationData(Guid id)
    {
        var query = new GetCustomerVerificationDataQuery { Id = id };
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    [HttpPost("lookup-by-numbers")]
    [ProducesResponseType(typeof(List<CustomerLookupDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> LookupByCustomerNumbers([FromBody] LookupByCustomerNumbersQuery query)
    {
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }
}

