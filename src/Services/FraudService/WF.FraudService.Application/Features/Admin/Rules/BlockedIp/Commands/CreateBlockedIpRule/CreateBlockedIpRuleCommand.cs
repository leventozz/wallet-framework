using MediatR;
using WF.FraudService.Domain.ValueObjects;
using WF.Shared.Contracts.Result;

namespace WF.FraudService.Application.Features.Admin.Rules.BlockedIp.Commands.CreateBlockedIpRule;

public record CreateBlockedIpRuleCommand(
    string IpAddress,
    string? Reason,
    DateTime? ExpiresAtUtc) : IRequest<Result<Guid>>;
