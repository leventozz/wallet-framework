using WF.FraudService.Application.Contracts;
using WF.FraudService.Application.Contracts.DTOs;
using WF.FraudService.Application.Features.FraudChecks.Commands.CheckFraud;

namespace WF.FraudService.Application.Features.FraudChecks.Rules;

public class RiskyHourFraudRule(IFraudRuleReadService _readService) : IFraudEvaluationRule
{
    public int Priority => 2;

    public async Task<FraudEvaluationResult> EvaluateAsync(CheckFraudCommandInternal request, CancellationToken cancellationToken)
    {
        var riskyHourRuleDtos = await _readService.GetActiveRiskyHourRulesAsync(cancellationToken);
        var currentTime = DateTime.UtcNow;
        
        foreach (var dto in riskyHourRuleDtos)
        {
            if (dto.IsCurrentTimeRisky(currentTime))
            {
                return new FraudEvaluationResult
                {
                    IsApproved = false,
                    FailureReason = $"Transaction attempted during risky hours ({dto.StartHour}-{dto.EndHour})"
                };
            }
        }

        return new FraudEvaluationResult { IsApproved = true };
    }
}

