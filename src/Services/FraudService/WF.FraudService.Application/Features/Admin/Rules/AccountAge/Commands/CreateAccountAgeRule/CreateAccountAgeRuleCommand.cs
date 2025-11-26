using MediatR;
using WF.FraudService.Domain.ValueObjects;
using WF.Shared.Contracts.Result;

namespace WF.FraudService.Application.Features.Admin.Rules.AccountAge.Commands.CreateAccountAgeRule;

public record CreateAccountAgeRuleCommand(
    int MinAccountAgeDays,
    Money? MaxAllowedAmount,
    string? Description) : IRequest<Result<Guid>>;
