using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WF.Shared.Contracts.Dtos;
using WF.Shared.Contracts.Result;
using WF.WalletService.Api.Controllers.Base;
using WF.WalletService.Application.Features.Wallets.Queries.GetWalletIdByCustomerIdAndCurrency;
using WF.WalletService.Application.Features.Wallets.Queries.LookupByCustomerIds;

namespace WF.WalletService.Api.Controllers;

[ApiVersion("1.0")]
public class WalletsController(IMediator _mediator) : BaseController
{
    [HttpGet("by-customer/{customerId:guid}/currency/{currency}")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWalletIdByCustomerIdAndCurrency(Guid customerId, string currency)
    {
        var query = new GetWalletIdByCustomerIdAndCurrencyQuery
        {
            CustomerId = customerId,
            Currency = currency
        };

        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    [HttpPost("lookup-by-customer-ids")]
    [ProducesResponseType(typeof(List<WalletLookupDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LookupByCustomerIds([FromBody] LookupByCustomerIdsQuery query)
    {
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }
}

