using MediatR;
using Microsoft.AspNetCore.Mvc;
using WF.TransactionService.Application.Features.Transfers.Commands.CreateTransfer;

namespace WF.TransactionService.Api.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class TransfersController(IMediator _mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTransfer([FromBody] CreateTransferCommand command, CancellationToken cancellationToken)
    {
        var correlationId = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(CreateTransfer), new { id = correlationId }, correlationId);
    }
}

