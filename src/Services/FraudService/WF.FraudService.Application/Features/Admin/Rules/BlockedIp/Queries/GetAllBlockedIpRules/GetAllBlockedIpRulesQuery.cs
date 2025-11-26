using MediatR;
using WF.FraudService.Application.Contracts.DTOs;
using WF.Shared.Contracts.Result;

namespace WF.FraudService.Application.Features.Admin.Rules.BlockedIp.Queries.GetAllBlockedIpRules;

public class GetAllBlockedIpRulesQuery : IRequest<Result<IEnumerable<BlockedIpRuleDto>>>
{
}
