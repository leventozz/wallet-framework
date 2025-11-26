using MediatR;
using WF.FraudService.Application.Contracts.DTOs;
using WF.Shared.Contracts.Result;

namespace WF.FraudService.Application.Features.Admin.Rules.AccountAge.Queries.GetAllAccountAgeRules;

public class GetAllAccountAgeRulesQuery : IRequest<Result<IEnumerable<AccountAgeRuleDto>>>
{
}
