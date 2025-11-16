using MediatR;
using Microsoft.Extensions.Logging;
using WF.FraudService.Application.Contracts;
using WF.Shared.Contracts.Abstractions;
using WF.Shared.Contracts.IntegrationEvents.Transaction;

namespace WF.FraudService.Application.Features.FraudChecks.Commands.CheckFraud;

public class CheckFraudCommandHandlerInternal(
    IEnumerable<IFraudEvaluationRule> _rules,
    IIntegrationEventPublisher _eventPublisher,
    IUnitOfWork _unitOfWork,
    ILogger<CheckFraudCommandHandlerInternal> _logger)
    : IRequestHandler<CheckFraudCommandInternal, bool>
{
    public async Task<bool> Handle(CheckFraudCommandInternal request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing fraud check for CorrelationId {CorrelationId}, SenderCustomerId {SenderCustomerId}, Amount {Amount}",
            request.CorrelationId,
            request.SenderCustomerId,
            request.Amount);

        var orderedRules = _rules.OrderBy(r => r.Priority);

        foreach (var rule in orderedRules)
        {
            var result = await rule.EvaluateAsync(request, cancellationToken);
            if (!result.IsApproved)
            {
                var declinedEvent = new FraudCheckDeclinedEvent
                {
                    CorrelationId = request.CorrelationId,
                    Reason = result.FailureReason
                };

                await _eventPublisher.PublishAsync(declinedEvent, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogWarning(
                    "Fraud check declined for CorrelationId {CorrelationId}, Reason: {Reason}",
                    request.CorrelationId,
                    result.FailureReason);

                return false;
            }
        }

        var approvedEvent = new FraudCheckApprovedEvent
        {
            CorrelationId = request.CorrelationId
        };

        await _eventPublisher.PublishAsync(approvedEvent, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Fraud check approved for CorrelationId {CorrelationId}", request.CorrelationId);

        return true;
    }
}
