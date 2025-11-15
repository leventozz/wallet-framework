using WF.FraudService.Application.Contracts;
using WF.FraudService.Application.Features.FraudChecks.Commands.CheckFraud;

namespace WF.FraudService.Application.Features.FraudChecks.Rules;

public class BlockedIpRule(IFraudRuleReadService _readService) : IFraudEvaluationRule
{
    public int Priority => 1;

    public async Task<FraudEvaluationResult> EvaluateAsync(CheckFraudCommandInternal request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.IpAddress))
        {
            return new FraudEvaluationResult { IsApproved = true };
        }

        var isBlocked = await _readService.IsIpBlockedAsync(request.IpAddress, cancellationToken);
        if (isBlocked)
        {
            return new FraudEvaluationResult
            {
                IsApproved = false,
                FailureReason = $"IP address {request.IpAddress} is blocked"
            };
        }

        return new FraudEvaluationResult { IsApproved = true };
    }
}

