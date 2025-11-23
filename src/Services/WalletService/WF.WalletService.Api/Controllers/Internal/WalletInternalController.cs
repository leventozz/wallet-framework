using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WF.Shared.Contracts.Dtos;
using WF.WalletService.Api.Controllers.Base;
using WF.WalletService.Application.Features.Wallets.Queries.GetWalletIdByCustomerIdAndCurrency;
using WF.WalletService.Application.Features.Wallets.Queries.LookupByCustomerIds;

namespace WF.WalletService.Api.Controllers.Internal;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/internal/wallets")]
[Authorize(Policy = "ServiceToService")]
[ApiExplorerSettings(IgnoreApi = true)]
public class WalletInternalController(IMediator _mediator) : BaseController
{
    [HttpGet("by-customer/{customerId:guid}/currency/{currency}")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWalletIdByCustomerIdAndCurrency(Guid customerId, string currency, CancellationToken cancellationToken)
    {
        var query = new GetWalletIdByCustomerIdAndCurrencyQuery
        {
            CustomerId = customerId,
            Currency = currency
        };

        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("lookup-by-customer-ids")]
    [ProducesResponseType(typeof(List<WalletLookupDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LookupByCustomerIds([FromBody] LookupByCustomerIdsQuery query, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }
}

