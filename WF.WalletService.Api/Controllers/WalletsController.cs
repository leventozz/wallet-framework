using MediatR;
using Microsoft.AspNetCore.Mvc;
using WF.WalletService.Application.Features.Wallets.Queries.GetWalletIdByCustomerIdAndCurrency;

namespace WF.WalletService.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
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
}

