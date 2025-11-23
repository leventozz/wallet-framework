using WF.FraudService.Application.Contracts;
using WF.FraudService.Application.Contracts.DTOs;
using WF.FraudService.Application.Features.FraudChecks.Commands.CheckFraud;
using WF.Shared.Contracts.Result;

namespace WF.FraudService.Application.Features.FraudChecks.Rules;

public class RiskyHourFraudRule(IFraudRuleReadService _readService) : IFraudEvaluationRule
{
    public int Priority => 2;

    public async Task<Result> EvaluateAsync(CheckFraudCommandInternal request, CancellationToken cancellationToken)
    {
        var riskyHourRuleDtos = await _readService.GetActiveRiskyHourRulesAsync(cancellationToken);
        var currentTime = DateTime.UtcNow;
        
        foreach (var dto in riskyHourRuleDtos)
        {
            if (dto.IsCurrentTimeRisky(currentTime))
            {
                return Result.Failure(Error.Failure("FraudCheck", $"Transaction attempted during risky hours ({dto.StartHour}-{dto.EndHour})"));
            }
        }

        return Result.Success();
    }
}

