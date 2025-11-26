using MediatR;
using WF.FraudService.Domain.Abstractions;
using WF.FraudService.Domain.Entities;
using WF.Shared.Contracts.Abstractions;
using WF.Shared.Contracts.Result;

namespace WF.FraudService.Application.Features.Admin.Rules.RiskyHour.Commands.CreateRiskyHourRule;

public class CreateRiskyHourRuleCommandHandler(
    IRiskyHourRuleRepository _repository,
    IUnitOfWork _unitOfWork) : IRequestHandler<CreateRiskyHourRuleCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateRiskyHourRuleCommand request, CancellationToken cancellationToken)
    {
        var ruleResult = RiskyHourRule.Create(
            request.TimeRange,
            request.Description);

        if (ruleResult.IsFailure)
        {
            return Result<Guid>.Failure(ruleResult.Error);
        }

        var rule = ruleResult.Value;
        await _repository.AddAsync(rule, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(rule.Id);
    }
}
