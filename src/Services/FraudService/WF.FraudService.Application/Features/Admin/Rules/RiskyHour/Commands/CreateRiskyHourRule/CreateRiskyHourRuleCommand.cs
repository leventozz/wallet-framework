using MediatR;
using WF.FraudService.Domain.ValueObjects;
using WF.Shared.Contracts.Result;

namespace WF.FraudService.Application.Features.Admin.Rules.RiskyHour.Commands.CreateRiskyHourRule;

public record CreateRiskyHourRuleCommand(
    TimeRange TimeRange,
    string? Description) : IRequest<Result<Guid>>;
