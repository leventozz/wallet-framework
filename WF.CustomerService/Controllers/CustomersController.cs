using MediatR;
using Microsoft.AspNetCore.Mvc;
using WF.CustomerService.Application.Dtos;
using WF.CustomerService.Application.Features.Customers.Commands.CreateCustomer;
using WF.CustomerService.Application.Features.Customers.Queries.GetCustomerById;

namespace WF.CustomerService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CustomersController(IMediator mediator)
        {
            _mediator = mediator;
        }

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
    }
}
