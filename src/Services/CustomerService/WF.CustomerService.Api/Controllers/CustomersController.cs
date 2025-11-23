using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WF.CustomerService.Api.Controllers.Base;
using WF.CustomerService.Application.Features.Customers.Commands.CreateCustomer;

namespace WF.CustomerService.Api.Controllers
{
    [ApiVersion("1.0")]
    [Authorize]
    public class CustomersController(IMediator _mediator) : BaseController
    {
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateCustomerCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsSuccess)
            {
                return HandleResultCreated(result, nameof(Create), new { id = result.Value });
            }
            return HandleResult(result);
        }

    }
}
