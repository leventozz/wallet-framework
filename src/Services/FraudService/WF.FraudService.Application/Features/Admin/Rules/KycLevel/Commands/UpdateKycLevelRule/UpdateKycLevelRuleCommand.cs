using MediatR;
using WF.FraudService.Domain.ValueObjects;
using WF.Shared.Contracts.Enums;
using WF.Shared.Contracts.Result;

namespace WF.FraudService.Application.Features.Admin.Rules.KycLevel.Commands.UpdateKycLevelRule;

public record UpdateKycLevelRuleCommand(
    Guid Id,
    KycStatus? RequiredKycStatus,
    Money? MaxAllowedAmount,
    string? Description,
    bool IsActive) : IRequest<Result>;
