using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WF.Shared.Contracts.Result;
using WF.TransactionService.Api.Controllers.Base;
using WF.TransactionService.Application.Dtos;
using WF.TransactionService.Application.Features.Admin.Queries.GetAdminTransactions;

namespace WF.TransactionService.Api.Controllers.Admin;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/transactions")]
[Authorize(Policy = "Support")]
public class AdminTransactionsController(IMediator _mediator) : BaseController
{
    [HttpPost("search")]
    [ProducesResponseType(typeof(PagedResult<AdminTransactionListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromBody] GetAdminTransactionsQuery query)
    {
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }
}
