using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WF.Shared.Contracts.Dtos;
using WF.WalletService.Application.Features.Wallets.Queries.GetWalletIdByCustomerIdAndCurrency;
using WF.WalletService.Application.Features.Wallets.Queries.LookupByCustomerIds;

namespace WF.WalletService.Api.Controllers;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("1.0")]
public class WalletsController(IMediator _mediator) : ControllerBase
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

        var walletId = await _mediator.Send(query);

        if (walletId == null)
        {
            return NotFound();
        }

        return Ok(walletId);
    }

    [HttpPost("lookup-by-customer-ids")]
    [ProducesResponseType(typeof(List<WalletLookupDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> LookupByCustomerIds([FromBody] LookupByCustomerIdsQuery query)
    {
        var results = await _mediator.Send(query);
        return Ok(results);
    }
}

