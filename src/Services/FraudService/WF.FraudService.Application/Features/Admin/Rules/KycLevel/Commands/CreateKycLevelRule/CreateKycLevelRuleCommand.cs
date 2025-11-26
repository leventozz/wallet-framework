using MediatR;
using WF.FraudService.Domain.ValueObjects;
using WF.Shared.Contracts.Enums;
using WF.Shared.Contracts.Result;

namespace WF.FraudService.Application.Features.Admin.Rules.KycLevel.Commands.CreateKycLevelRule;

public record CreateKycLevelRuleCommand(
    KycStatus RequiredKycStatus,
    Money? MaxAllowedAmount,
    string? Description) : IRequest<Result<Guid>>;
