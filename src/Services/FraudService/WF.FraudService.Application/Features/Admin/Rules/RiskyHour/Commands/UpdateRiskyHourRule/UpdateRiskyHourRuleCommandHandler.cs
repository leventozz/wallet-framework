using MediatR;
using WF.FraudService.Domain.Abstractions;
using WF.Shared.Contracts.Abstractions;
using WF.Shared.Contracts.Result;

namespace WF.FraudService.Application.Features.Admin.Rules.RiskyHour.Commands.UpdateRiskyHourRule;

public class UpdateRiskyHourRuleCommandHandler(
    IRiskyHourRuleRepository _repository,
    IUnitOfWork _unitOfWork) : IRequestHandler<UpdateRiskyHourRuleCommand, Result>
{
    public async Task<Result> Handle(UpdateRiskyHourRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (rule is null)
        {
            return Result.Failure(Error.NotFound("RiskyHourRule", request.Id));
        }

        if (request.TimeRange.HasValue)
        {
            rule.UpdateHours(request.TimeRange.Value);
        }

        if (request.Description is not null)
        {
            rule.UpdateDescription(request.Description);
        }

        if (request.IsActive)
        {
            rule.Activate();
        }
        else
        {
            rule.Deactivate();
        }

        await _repository.UpdateAsync(rule, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
