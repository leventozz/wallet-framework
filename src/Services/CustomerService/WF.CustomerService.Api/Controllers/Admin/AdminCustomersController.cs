using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WF.CustomerService.Api.Controllers.Base;
using WF.CustomerService.Application.Features.Customers.Queries.GetAllCustomersWithWallets;
using WF.CustomerService.Application.Features.Customers.Commands.CloseCustomer;
using WF.Shared.Contracts.Result;

namespace WF.CustomerService.Api.Controllers.Admin;


[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/customers")]
[Authorize(Policy = "Support")]
public class AdminCustomersController(IMediator _mediator) : BaseController
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AdminCustomerListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var query = new GetAllCustomersWithWalletsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    [HttpPost("{customerId}/close")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Policy = "Officer")]
    public async Task<IActionResult> CloseCustomer(Guid customerId)
    {
        var command = new CloseCustomerCommand(customerId);
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }
}
