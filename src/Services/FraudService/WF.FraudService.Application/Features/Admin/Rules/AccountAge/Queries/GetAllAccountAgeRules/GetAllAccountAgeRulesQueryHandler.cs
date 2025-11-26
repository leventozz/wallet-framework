using MediatR;
using WF.FraudService.Application.Contracts;
using WF.FraudService.Application.Contracts.DTOs;
using WF.Shared.Contracts.Result;

namespace WF.FraudService.Application.Features.Admin.Rules.AccountAge.Queries.GetAllAccountAgeRules;

public class GetAllAccountAgeRulesQueryHandler(IAdminFraudRuleQueryService _queryService) 
    : IRequestHandler<GetAllAccountAgeRulesQuery, Result<IEnumerable<AccountAgeRuleDto>>>
{
    public async Task<Result<IEnumerable<AccountAgeRuleDto>>> Handle(GetAllAccountAgeRulesQuery request, CancellationToken cancellationToken)
    {
        var rules = await _queryService.GetAllAccountAgeRulesAsync(cancellationToken);
        return Result<IEnumerable<AccountAgeRuleDto>>.Success(rules);
    }
}
