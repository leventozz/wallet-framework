using MediatR;
using WF.FraudService.Domain.ValueObjects;
using WF.Shared.Contracts.Result;

namespace WF.FraudService.Application.Features.Admin.Rules.RiskyHour.Commands.UpdateRiskyHourRule;

public record UpdateRiskyHourRuleCommand(
    Guid Id,
    TimeRange? TimeRange,
    string? Description,
    bool IsActive) : IRequest<Result>;
