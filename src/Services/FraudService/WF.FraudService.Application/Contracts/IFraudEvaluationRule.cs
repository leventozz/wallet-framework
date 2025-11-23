using WF.FraudService.Application.Features.FraudChecks.Commands.CheckFraud;
using WF.Shared.Contracts.Result;

namespace WF.FraudService.Application.Contracts;

public interface IFraudEvaluationRule
{
    int Priority { get; }
    Task<Result> EvaluateAsync(CheckFraudCommandInternal request, CancellationToken cancellationToken);
}

