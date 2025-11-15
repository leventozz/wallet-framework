using MediatR;

namespace WF.FraudService.Application.Features.FraudChecks.Commands.CheckFraud;

public record CheckFraudCommandInternal : IRequest<bool>
{
    public Guid CorrelationId { get; init; }
    public Guid SenderCustomerId { get; init; }
    public Guid ReceiverCustomerId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string? IpAddress { get; init; }
}

