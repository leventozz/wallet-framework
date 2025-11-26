using MediatR;
using WF.FraudService.Application.Contracts.DTOs;
using WF.Shared.Contracts.Result;

namespace WF.FraudService.Application.Features.Admin.Rules.RiskyHour.Queries.GetAllRiskyHourRules;

public class GetAllRiskyHourRulesQuery : IRequest<Result<IEnumerable<RiskyHourRuleDto>>>
{
}
