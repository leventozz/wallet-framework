using MediatR;
using WF.FraudService.Application.Contracts;
using WF.FraudService.Application.Contracts.DTOs;
using WF.Shared.Contracts.Result;

namespace WF.FraudService.Application.Features.Admin.Rules.RiskyHour.Queries.GetAllRiskyHourRules;

public class GetAllRiskyHourRulesQueryHandler(IAdminFraudRuleQueryService _queryService) 
    : IRequestHandler<GetAllRiskyHourRulesQuery, Result<IEnumerable<RiskyHourRuleDto>>>
{
    public async Task<Result<IEnumerable<RiskyHourRuleDto>>> Handle(GetAllRiskyHourRulesQuery request, CancellationToken cancellationToken)
    {
        var rules = await _queryService.GetAllRiskyHourRulesAsync(cancellationToken);
        return Result<IEnumerable<RiskyHourRuleDto>>.Success(rules);
    }
}
