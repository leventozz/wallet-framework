using MediatR;
using WF.Shared.Contracts.Result;

namespace WF.FraudService.Application.Features.Admin.Rules.BlockedIp.Commands.UpdateBlockedIpRule;

public record UpdateBlockedIpRuleCommand(
    Guid Id,
    string? Reason,
    bool IsActive) : IRequest<Result>;
