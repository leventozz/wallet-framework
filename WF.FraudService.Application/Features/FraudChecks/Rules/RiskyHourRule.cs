using WF.FraudService.Application.Contracts;
using WF.FraudService.Application.Features.FraudChecks.Commands.CheckFraud;

namespace WF.FraudService.Application.Features.FraudChecks.Rules;

public class RiskyHourRule(IFraudRuleReadService _readService) : IFraudEvaluationRule
{
    public int Priority => 2;

    public async Task<FraudEvaluationResult> EvaluateAsync(CheckFraudCommandInternal request, CancellationToken cancellationToken)
    {
        var riskyHourRules = await _readService.GetActiveRiskyHourRulesAsync(cancellationToken);
        
        foreach (var rule in riskyHourRules)
        {
            if (rule.IsInRiskyHour(DateTime.UtcNow))
            {
                return new FraudEvaluationResult
                {
                    IsApproved = false,
                    FailureReason = $"Transaction attempted during risky hours ({rule.StartHour}-{rule.EndHour})"
                };
            }
        }

        return new FraudEvaluationResult { IsApproved = true };
    }
}

