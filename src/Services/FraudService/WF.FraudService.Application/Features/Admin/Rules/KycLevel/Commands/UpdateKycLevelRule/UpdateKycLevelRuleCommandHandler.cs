using MediatR;
using WF.FraudService.Domain.Abstractions;
using WF.Shared.Contracts.Abstractions;
using WF.Shared.Contracts.Result;

namespace WF.FraudService.Application.Features.Admin.Rules.KycLevel.Commands.UpdateKycLevelRule;

public class UpdateKycLevelRuleCommandHandler(
    IKycLevelRuleRepository _repository,
    IUnitOfWork _unitOfWork) : IRequestHandler<UpdateKycLevelRuleCommand, Result>
{
    public async Task<Result> Handle(UpdateKycLevelRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (rule is null)
        {
            return Result.Failure(Error.NotFound("KycLevelRule.NotFound", $"KYC level rule with ID {request.Id} not found."));
        }

        if (request.RequiredKycStatus.HasValue)
        {
            rule.UpdateRequiredKycStatus(request.RequiredKycStatus.Value);
        }

        if (request.MaxAllowedAmount is not null)
        {
            rule.UpdateMaxAllowedAmount(request.MaxAllowedAmount);
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
