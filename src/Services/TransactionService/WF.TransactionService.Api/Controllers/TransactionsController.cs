using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WF.TransactionService.Api.Controllers.Base;
using WF.TransactionService.Application.Features.Transactions.Commands.CreateTransaction;

namespace WF.TransactionService.Api.Controllers;

[ApiVersion("1.0")]
[Authorize]
public class TransactionsController(IMediator _mediator) : BaseController
{
    [HttpPost]
    [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionCommand command, CancellationToken cancellationToken)
    {
        command.SenderIdentityId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResultCreated(result, nameof(CreateTransaction), result.IsSuccess ? new { id = result.Value } : null);
    }
}

