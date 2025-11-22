using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WF.CustomerService.Application.Features.Customers.Commands.CreateCustomer;
using WF.CustomerService.Application.Features.Customers.Queries.GetCustomerById;
using WF.CustomerService.Application.Features.Customers.Queries.GetCustomerByCustomerNo;
using WF.CustomerService.Application.Features.Customers.Queries.GetCustomerVerificationData;
using WF.CustomerService.Application.Features.Customers.Queries.GetCustomerIdByCustomerNumber;
using WF.CustomerService.Application.Features.Customers.Queries.GetCustomerByIdentity;
using WF.CustomerService.Application.Features.Customers.Queries.LookupByCustomerNumbers;
using WF.Shared.Contracts.Dtos;
using WF.CustomerService.Api.Controllers.Base;

namespace WF.CustomerService.Api.Controllers
{
    [ApiVersion("1.0")]
    public class CustomersController(IMediator _mediator) : BaseController
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
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var query = new GetCustomerByIdQuery { Id = id };
            var result = await _mediator.Send(query, cancellationToken);
            return HandleResult(result);
        }

        [HttpGet("by-identity/{identityId}")]
        [ProducesResponseType(typeof(CustomerLookupDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCustomerByIdentity(string identityId, CancellationToken cancellationToken)
        {
            var query = new GetCustomerByIdentityQuery { IdentityId = identityId };
            var result = await _mediator.Send(query, cancellationToken);
            return HandleResult(result);
        }

        [HttpGet("number/{customerNumber}")]
        [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByCustomerNo(string customerNumber)
        {
            var query = new GetCustomerByCustomerNoQuery { CustomerNumber = customerNumber };
            var result = await _mediator.Send(query);
            return HandleResult(result);
        }

        [HttpGet("number/{customerNumber}/id")]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetIdByCustomerNumber(string customerNumber)
        {
            var query = new GetCustomerIdByCustomerNumberQuery { CustomerNumber = customerNumber };
            var result = await _mediator.Send(query);
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
}
