using MediatR;
using WF.FraudService.Domain.Abstractions;
using WF.FraudService.Domain.Entities;
using WF.Shared.Contracts.Abstractions;
using WF.Shared.Contracts.Result;

namespace WF.FraudService.Application.Features.Admin.Rules.KycLevel.Commands.CreateKycLevelRule;

public class CreateKycLevelRuleCommandHandler(
    IKycLevelRuleRepository _repository,
    IUnitOfWork _unitOfWork) : IRequestHandler<CreateKycLevelRuleCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateKycLevelRuleCommand request, CancellationToken cancellationToken)
    {
        var ruleResult = KycLevelRule.Create(
            request.RequiredKycStatus,
            request.MaxAllowedAmount,
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
