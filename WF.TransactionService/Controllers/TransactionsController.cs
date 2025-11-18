using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WF.TransactionService.Application.Features.Transactions.Commands.CreateTransaction;

namespace WF.TransactionService.Api.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Authorize]
public class TransactionsController(IMediator _mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionCommand command, CancellationToken cancellationToken)
    {
        var correlationId = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(CreateTransaction), new { id = correlationId }, correlationId);
    }
}

