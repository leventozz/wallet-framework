using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WF.Shared.Contracts.Result;
using WF.WalletService.Api.Controllers.Base;
using WF.WalletService.Application.Dtos;
using WF.WalletService.Application.Features.Admin.Queries.GetAdminWallets;
using WF.WalletService.Application.Features.Admin.Commands.CloseWallet;
using WF.WalletService.Application.Features.Admin.Commands.FreezeWallet;

namespace WF.WalletService.Api.Controllers.Admin;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/wallets")]
[Authorize(Policy = "Support")]
public class AdminWalletsController(IMediator _mediator) : BaseController
{
    [HttpPost("search")]
    [ProducesResponseType(typeof(PagedResult<AdminWalletListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromBody] GetAdminWalletsQuery query)
    {
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    [HttpPost("{id}/close")]
    [Authorize(Policy = "Officer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Close([FromRoute] Guid id)
    {
        var command = new CloseWalletCommand(id);
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPost("{id}/freeze")]
    [Authorize(Policy = "Officer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Freeze([FromRoute] Guid id)
    {
        var command = new FreezeWalletCommand(id);
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }
}
