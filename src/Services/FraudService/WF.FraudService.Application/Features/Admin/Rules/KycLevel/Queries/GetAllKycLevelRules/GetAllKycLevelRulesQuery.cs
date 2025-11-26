using MediatR;
using WF.FraudService.Application.Contracts.DTOs;
using WF.Shared.Contracts.Result;

namespace WF.FraudService.Application.Features.Admin.Rules.KycLevel.Queries.GetAllKycLevelRules;

public class GetAllKycLevelRulesQuery : IRequest<Result<IEnumerable<KycLevelRuleDto>>>
{
}
