using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WF.FraudService.Api.Controllers.Base;
using WF.FraudService.Application.Contracts.DTOs;
using WF.FraudService.Application.Features.Admin.Rules.AccountAge.Commands.CreateAccountAgeRule;
using WF.FraudService.Application.Features.Admin.Rules.AccountAge.Commands.UpdateAccountAgeRule;
using WF.FraudService.Application.Features.Admin.Rules.AccountAge.Queries.GetAllAccountAgeRules;
using WF.FraudService.Application.Features.Admin.Rules.BlockedIp.Commands.CreateBlockedIpRule;
using WF.FraudService.Application.Features.Admin.Rules.BlockedIp.Commands.UpdateBlockedIpRule;
using WF.FraudService.Application.Features.Admin.Rules.BlockedIp.Queries.GetAllBlockedIpRules;
using WF.FraudService.Application.Features.Admin.Rules.KycLevel.Commands.CreateKycLevelRule;
using WF.FraudService.Application.Features.Admin.Rules.KycLevel.Commands.UpdateKycLevelRule;
using WF.FraudService.Application.Features.Admin.Rules.KycLevel.Queries.GetAllKycLevelRules;
using WF.FraudService.Application.Features.Admin.Rules.RiskyHour.Commands.CreateRiskyHourRule;
using WF.FraudService.Application.Features.Admin.Rules.RiskyHour.Commands.UpdateRiskyHourRule;
using WF.FraudService.Application.Features.Admin.Rules.RiskyHour.Queries.GetAllRiskyHourRules;
using WF.Shared.Contracts.Result;

namespace WF.FraudService.Api.Controllers.Admin;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/fraud")]
[Authorize(Policy = "Support")]
public class AdminFraudController(IMediator _mediator) : BaseController
{
    [HttpGet("account-age-rules")]
    [ProducesResponseType(typeof(IEnumerable<AccountAgeRuleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAccountAgeRules()
    {
        var result = await _mediator.Send(new GetAllAccountAgeRulesQuery());
        return HandleResult(result);
    }

    [HttpPost("account-age-rules")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [Authorize(Policy = "Officer")]
    public async Task<IActionResult> CreateAccountAgeRule([FromBody] CreateAccountAgeRuleCommand command)
    {
        var result = await _mediator.Send(command);
        return HandleResultCreated(result, nameof(GetAllAccountAgeRules), new { });
    }

    [HttpPut("account-age-rules/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize(Policy = "Officer")]
    public async Task<IActionResult> UpdateAccountAgeRule(Guid id, [FromBody] UpdateAccountAgeRuleCommand command)
    {
        if (id != command.Id)
            return BadRequest("ID mismatch");

        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    [HttpGet("blocked-ip-rules")]
    [ProducesResponseType(typeof(IEnumerable<BlockedIpRuleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllBlockedIpRules()
    {
        var result = await _mediator.Send(new GetAllBlockedIpRulesQuery());
        return HandleResult(result);
    }

    [HttpPost("blocked-ip-rules")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [Authorize(Policy = "Officer")]
    public async Task<IActionResult> CreateBlockedIpRule([FromBody] CreateBlockedIpRuleCommand command)
    {
        var result = await _mediator.Send(command);
        return HandleResultCreated(result, nameof(GetAllBlockedIpRules), new { });
    }

    [HttpPut("blocked-ip-rules/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize(Policy = "Officer")]
    public async Task<IActionResult> UpdateBlockedIpRule(Guid id, [FromBody] UpdateBlockedIpRuleCommand command)
    {
        if (id != command.Id)
            return BadRequest("ID mismatch");

        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    [HttpGet("kyc-level-rules")]
    [ProducesResponseType(typeof(IEnumerable<KycLevelRuleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllKycLevelRules()
    {
        var result = await _mediator.Send(new GetAllKycLevelRulesQuery());
        return HandleResult(result);
    }

    [HttpPost("kyc-level-rules")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [Authorize(Policy = "Officer")]
    public async Task<IActionResult> CreateKycLevelRule([FromBody] CreateKycLevelRuleCommand command)
    {
        var result = await _mediator.Send(command);
        return HandleResultCreated(result, nameof(GetAllKycLevelRules), new { });
    }

    [HttpPut("kyc-level-rules/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize(Policy = "Officer")]
    public async Task<IActionResult> UpdateKycLevelRule(Guid id, [FromBody] UpdateKycLevelRuleCommand command)
    {
        if (id != command.Id)
            return BadRequest("ID mismatch");

        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    [HttpGet("risky-hour-rules")]
    [ProducesResponseType(typeof(IEnumerable<RiskyHourRuleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllRiskyHourRules()
    {
        var result = await _mediator.Send(new GetAllRiskyHourRulesQuery());
        return HandleResult(result);
    }

    [HttpPost("risky-hour-rules")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [Authorize(Policy = "Officer")]
    public async Task<IActionResult> CreateRiskyHourRule([FromBody] CreateRiskyHourRuleCommand command)
    {
        var result = await _mediator.Send(command);
        return HandleResultCreated(result, nameof(GetAllRiskyHourRules), new { });
    }

    [HttpPut("risky-hour-rules/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize(Policy = "Officer")]
    public async Task<IActionResult> UpdateRiskyHourRule(Guid id, [FromBody] UpdateRiskyHourRuleCommand command)
    {
        if (id != command.Id)
            return BadRequest("ID mismatch");

        var result = await _mediator.Send(command);
        return HandleResult(result);
    }
}
