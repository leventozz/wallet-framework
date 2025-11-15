using WF.FraudService.Application.Features.FraudChecks.Commands.CheckFraud;

namespace WF.FraudService.Application.Contracts;

public interface IFraudEvaluationRule
{
    int Priority { get; }
    Task<FraudEvaluationResult> EvaluateAsync(CheckFraudCommandInternal request, CancellationToken cancellationToken);
}

public class FraudEvaluationResult
{
    public bool IsApproved { get; set; }
    public string FailureReason { get; set; } = string.Empty;
}

