using MediatR;
using WF.FraudService.Domain.ValueObjects;
using WF.Shared.Contracts.Result;

namespace WF.FraudService.Application.Features.Admin.Rules.AccountAge.Commands.UpdateAccountAgeRule;

public record UpdateAccountAgeRuleCommand(
    Guid Id,
    int? MinAccountAgeDays,
    Money? MaxAllowedAmount,
    string? Description,
    bool IsActive) : IRequest<Result>;
