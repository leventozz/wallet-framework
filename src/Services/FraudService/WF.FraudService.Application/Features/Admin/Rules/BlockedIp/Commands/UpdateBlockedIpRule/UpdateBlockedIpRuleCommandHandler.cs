using MediatR;
using WF.FraudService.Domain.Abstractions;
using WF.Shared.Contracts.Abstractions;
using WF.Shared.Contracts.Result;

namespace WF.FraudService.Application.Features.Admin.Rules.BlockedIp.Commands.UpdateBlockedIpRule;

public class UpdateBlockedIpRuleCommandHandler(
    IBlockedIpRepository _repository,
    IUnitOfWork _unitOfWork) : IRequestHandler<UpdateBlockedIpRuleCommand, Result>
{
    public async Task<Result> Handle(UpdateBlockedIpRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (rule is null)
        {
            return Result.Failure(Error.NotFound("BlockedIpRule.NotFound", $"Blocked IP rule with ID {request.Id} not found."));
        }

        if (request.Reason is not null)
        {
            rule.UpdateReason(request.Reason);
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
