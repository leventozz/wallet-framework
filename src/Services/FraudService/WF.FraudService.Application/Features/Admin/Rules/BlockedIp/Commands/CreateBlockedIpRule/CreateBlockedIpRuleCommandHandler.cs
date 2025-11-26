using MediatR;
using WF.FraudService.Domain.Abstractions;
using WF.FraudService.Domain.Entities;
using WF.FraudService.Domain.ValueObjects;
using WF.Shared.Contracts.Abstractions;
using WF.Shared.Contracts.Result;

namespace WF.FraudService.Application.Features.Admin.Rules.BlockedIp.Commands.CreateBlockedIpRule;

public class CreateBlockedIpRuleCommandHandler(
    IBlockedIpRepository _repository,
    IUnitOfWork _unitOfWork) : IRequestHandler<CreateBlockedIpRuleCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateBlockedIpRuleCommand request, CancellationToken cancellationToken)
    {
        var ipResult = IpAddress.Create(request.IpAddress);
        if (ipResult.IsFailure)
        {
            return Result<Guid>.Failure(ipResult.Error);
        }

        var ruleResult = BlockedIpRule.Create(
            ipResult.Value,
            request.Reason,
            request.ExpiresAtUtc);

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
