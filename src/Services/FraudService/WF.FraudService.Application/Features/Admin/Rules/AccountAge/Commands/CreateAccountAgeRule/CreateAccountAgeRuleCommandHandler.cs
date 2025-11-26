using MediatR;
using WF.FraudService.Domain.Abstractions;
using WF.FraudService.Domain.Entities;
using WF.Shared.Contracts.Abstractions;
using WF.Shared.Contracts.Result;

namespace WF.FraudService.Application.Features.Admin.Rules.AccountAge.Commands.CreateAccountAgeRule;

public class CreateAccountAgeRuleCommandHandler(
    IAccountAgeRuleRepository _repository,
    IUnitOfWork _unitOfWork) : IRequestHandler<CreateAccountAgeRuleCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateAccountAgeRuleCommand request, CancellationToken cancellationToken)
    {
        var ruleResult = AccountAgeRule.Create(
            request.MinAccountAgeDays,
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
