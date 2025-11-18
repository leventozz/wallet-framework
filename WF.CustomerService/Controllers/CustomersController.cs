using MediatR;
using Microsoft.AspNetCore.Mvc;
using WF.CustomerService.Application.Features.Customers.Commands.CreateCustomer;
using WF.CustomerService.Application.Features.Customers.Queries.GetCustomerById;
using WF.CustomerService.Application.Features.Customers.Queries.GetCustomerByCustomerNo;
using WF.CustomerService.Application.Features.Customers.Queries.GetCustomerVerificationData;
using WF.CustomerService.Application.Features.Customers.Queries.GetCustomerIdByCustomerNumber;
using WF.Shared.Contracts.Dtos;

namespace WF.CustomerService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController(IMediator _mediator) : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] CreateCustomerCommand command)
        {
            var customerId = await _mediator.Send(command);
            return CreatedAtAction(nameof(Create), new { id = customerId }, customerId);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var query = new GetCustomerByIdQuery { Id = id };
            var dto = await _mediator.Send(query);
            return Ok(dto);
        }

        [HttpGet("number/{customerNumber}")]
        [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByCustomerNo(string customerNumber)
        {
            var query = new GetCustomerByCustomerNoQuery { CustomerNumber = customerNumber };
            var dto = await _mediator.Send(query);
            return Ok(dto);
        }

        [HttpGet("number/{customerNumber}/id")]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetIdByCustomerNumber(string customerNumber)
        {
            var query = new GetCustomerIdByCustomerNumberQuery { CustomerNumber = customerNumber };
            var customerId = await _mediator.Send(query);
            
            if (customerId == null)
            {
                return NotFound();
            }
            
            return Ok(customerId);
        }

        [HttpGet("/api/v1/customers/{id:guid}/verification-data")]
        [ProducesResponseType(typeof(CustomerVerificationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetVerificationData(Guid id)
        {
            var query = new GetCustomerVerificationDataQuery { Id = id };
            var dto = await _mediator.Send(query);
            return Ok(dto);
        }
    }
}
