using WF.FraudService.Application.Contracts;
using WF.FraudService.Application.Features.FraudChecks.Commands.CheckFraud;

namespace WF.FraudService.Application.Features.FraudChecks.Rules;

public class KycLevelRule(IFraudRuleReadService _readService) : IFraudEvaluationRule
{
    public int Priority => 4;

    public async Task<FraudEvaluationResult> EvaluateAsync(CheckFraudCommandInternal request, CancellationToken cancellationToken)
    {
        var kycLevelRules = await _readService.GetActiveKycLevelRulesAsync(cancellationToken);
        
        foreach (var rule in kycLevelRules)
        {
            if (rule.MaxAllowedAmount.HasValue && request.Amount > rule.MaxAllowedAmount.Value)
            {
                return new FraudEvaluationResult
                {
                    IsApproved = false,
                    FailureReason = $"Amount {request.Amount} exceeds maximum allowed amount {rule.MaxAllowedAmount.Value} for KYC level rule"
                };
            }
        }

        return new FraudEvaluationResult { IsApproved = true };
    }
}

