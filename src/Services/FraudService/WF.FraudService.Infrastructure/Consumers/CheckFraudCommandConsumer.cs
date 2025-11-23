using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using WF.FraudService.Application.Features.FraudChecks.Commands.CheckFraud;
using WF.Shared.Contracts.Commands.Fraud;

namespace WF.FraudService.Infrastructure.Consumers;

public class CheckFraudCommandConsumer(
    IMediator _mediator,
    ILogger<CheckFraudCommandConsumer> _logger)
    : IConsumer<CheckFraudCommandContract>
{
    public async Task Consume(ConsumeContext<CheckFraudCommandContract> context)
    {
        var command = context.Message;

        _logger.LogInformation(
            "CheckFraudCommandContract received for CorrelationId {CorrelationId}, SenderCustomerId {SenderCustomerId}, Amount {Amount}",
            command.CorrelationId,
            command.SenderCustomerId,
            command.Amount);

        var handlerCommand = new CheckFraudCommandInternal
        {
            CorrelationId = command.CorrelationId,
            SenderCustomerId = command.SenderCustomerId,
            ReceiverCustomerId = command.ReceiverCustomerId,
            Amount = command.Amount,
            Currency = command.Currency,
            IpAddress = null
        };

        await _mediator.Send(handlerCommand, context.CancellationToken);

        _logger.LogInformation(
            "CheckFraudCommandContract processed successfully for CorrelationId {CorrelationId}",
            command.CorrelationId);
    }
}

