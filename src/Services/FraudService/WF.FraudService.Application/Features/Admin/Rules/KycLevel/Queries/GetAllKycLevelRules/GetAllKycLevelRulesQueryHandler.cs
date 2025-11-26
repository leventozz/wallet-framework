using MediatR;
using WF.FraudService.Application.Contracts;
using WF.FraudService.Application.Contracts.DTOs;
using WF.Shared.Contracts.Result;

namespace WF.FraudService.Application.Features.Admin.Rules.KycLevel.Queries.GetAllKycLevelRules;

public class GetAllKycLevelRulesQueryHandler(IAdminFraudRuleQueryService _queryService) 
    : IRequestHandler<GetAllKycLevelRulesQuery, Result<IEnumerable<KycLevelRuleDto>>>
{
    public async Task<Result<IEnumerable<KycLevelRuleDto>>> Handle(GetAllKycLevelRulesQuery request, CancellationToken cancellationToken)
    {
        var rules = await _queryService.GetAllKycLevelRulesAsync(cancellationToken);
        return Result<IEnumerable<KycLevelRuleDto>>.Success(rules);
    }
}
