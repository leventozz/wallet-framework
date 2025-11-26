using WF.FraudService.Application.Contracts;
using WF.FraudService.Application.Contracts.DTOs;
using WF.FraudService.Application.Features.FraudChecks.Commands.CheckFraud;
using WF.Shared.Contracts.Result;

namespace WF.FraudService.Application.Features.FraudChecks.Rules;

public class BlockedIpFraudRule(
    IFraudRuleReadService _readService,
    ITimeProvider _timeProvider) : IFraudEvaluationRule
{
    public int Priority => 1;

    public async Task<Result> EvaluateAsync(CheckFraudCommandInternal request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.IpAddress))
        {
            return Result.Success();
        }

        var blockedIpRule = await _readService.GetBlockedIpRuleAsync(request.IpAddress, cancellationToken);
        if (blockedIpRule.IsBlocked(_timeProvider.UtcNow))
        {
            return Result.Failure(Error.Failure("FraudCheck", $"IP address {request.IpAddress} is blocked"));
        }

        return Result.Success();
    }
}

