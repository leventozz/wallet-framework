using WF.FraudService.Application.Contracts;
using WF.FraudService.Application.Features.FraudChecks.Commands.CheckFraud;

namespace WF.FraudService.Application.Features.FraudChecks.Rules;

public class AccountAgeRule(IFraudRuleReadService _readService) : IFraudEvaluationRule
{
    public int Priority => 3;

    public async Task<FraudEvaluationResult> EvaluateAsync(CheckFraudCommandInternal request, CancellationToken cancellationToken)
    {
        var accountAgeRules = await _readService.GetActiveAccountAgeRulesAsync(cancellationToken);
        
        foreach (var rule in accountAgeRules)
        {
            if (rule.MaxAllowedAmount.HasValue && request.Amount > rule.MaxAllowedAmount.Value)
            {
                return new FraudEvaluationResult
                {
                    IsApproved = false,
                    FailureReason = $"Amount {request.Amount} exceeds maximum allowed amount {rule.MaxAllowedAmount.Value} for account age rule"
                };
            }
        }

        return new FraudEvaluationResult { IsApproved = true };
    }
}

