using MediatR;
using WF.FraudService.Application.Contracts;
using WF.FraudService.Application.Contracts.DTOs;
using WF.Shared.Contracts.Result;

namespace WF.FraudService.Application.Features.Admin.Rules.BlockedIp.Queries.GetAllBlockedIpRules;

public class GetAllBlockedIpRulesQueryHandler(IAdminFraudRuleQueryService _queryService) 
    : IRequestHandler<GetAllBlockedIpRulesQuery, Result<IEnumerable<BlockedIpRuleDto>>>
{
    public async Task<Result<IEnumerable<BlockedIpRuleDto>>> Handle(GetAllBlockedIpRulesQuery request, CancellationToken cancellationToken)
    {
        var rules = await _queryService.GetAllBlockedIpRulesAsync(cancellationToken);
        return Result<IEnumerable<BlockedIpRuleDto>>.Success(rules);
    }
}
