using MediatR;
using WF.FraudService.Application.Contracts;
using WF.Shared.Abstractions;
using WF.Shared.Contracts.IntegrationEvents.Transaction;
using Microsoft.Extensions.Logging;

namespace WF.FraudService.Application.Features.FraudChecks.Commands.CheckFraud;

public class CheckFraudCommandHandlerInternal(
    IFraudRuleReadService _readService,
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

        var isApproved = await EvaluateFraudRulesAsync(request, cancellationToken);

        if (isApproved)
        {
            var responseEvent = new FraudCheckApprovedEvent
            {
                CorrelationId = request.CorrelationId
            };

            await _eventPublisher.PublishAsync(responseEvent, cancellationToken);
            _logger.LogInformation("Fraud check approved for CorrelationId {CorrelationId}", request.CorrelationId);
        }
        else
        {
            var reason = await GetDeclineReasonAsync(request, cancellationToken);
            var responseEvent = new FraudCheckDeclinedEvent
            {
                CorrelationId = request.CorrelationId,
                Reason = reason
            };

            await _eventPublisher.PublishAsync(responseEvent, cancellationToken);
            _logger.LogWarning(
                "Fraud check declined for CorrelationId {CorrelationId}, Reason: {Reason}",
                request.CorrelationId,
                reason);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return isApproved;
    }

    private async Task<bool> EvaluateFraudRulesAsync(CheckFraudCommandInternal request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.IpAddress))
        {
            var isIpBlocked = await _readService.IsIpBlockedAsync(request.IpAddress, cancellationToken);
            if (isIpBlocked)
            {
                return false;
            }
        }

        var riskyHourRules = await _readService.GetActiveRiskyHourRulesAsync(cancellationToken);
        foreach (var rule in riskyHourRules)
        {
            if (rule.IsInRiskyHour(DateTime.UtcNow))
            {
                return false;
            }
        }

        var accountAgeRules = await _readService.GetActiveAccountAgeRulesAsync(cancellationToken);
        foreach (var rule in accountAgeRules)
        {
            if (rule.MaxAllowedAmount.HasValue && request.Amount > rule.MaxAllowedAmount.Value)
            {
                return false;
            }
        }

        var kycLevelRules = await _readService.GetActiveKycLevelRulesAsync(cancellationToken);
        foreach (var rule in kycLevelRules)
        {
            if (rule.MaxAllowedAmount.HasValue && request.Amount > rule.MaxAllowedAmount.Value)
            {
                return false;
            }
        }

        return true;
    }

    private async Task<string> GetDeclineReasonAsync(CheckFraudCommandInternal request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.IpAddress))
        {
            var isIpBlocked = await _readService.IsIpBlockedAsync(request.IpAddress, cancellationToken);
            if (isIpBlocked)
            {
                return $"IP address {request.IpAddress} is blocked";
            }
        }

        var riskyHourRules = await _readService.GetActiveRiskyHourRulesAsync(cancellationToken);
        foreach (var rule in riskyHourRules)
        {
            if (rule.IsInRiskyHour(DateTime.UtcNow))
            {
                return $"Transaction attempted during risky hours ({rule.StartHour}-{rule.EndHour})";
            }
        }

        var accountAgeRules = await _readService.GetActiveAccountAgeRulesAsync(cancellationToken);
        foreach (var rule in accountAgeRules)
        {
            if (rule.MaxAllowedAmount.HasValue && request.Amount > rule.MaxAllowedAmount.Value)
            {
                return $"Amount {request.Amount} exceeds maximum allowed amount {rule.MaxAllowedAmount.Value} for account age rule";
            }
        }

        var kycLevelRules = await _readService.GetActiveKycLevelRulesAsync(cancellationToken);
        foreach (var rule in kycLevelRules)
        {
            if (rule.MaxAllowedAmount.HasValue && request.Amount > rule.MaxAllowedAmount.Value)
            {
                return $"Amount {request.Amount} exceeds maximum allowed amount {rule.MaxAllowedAmount.Value} for KYC level rule";
            }
        }

        return "Fraud check failed";
    }
}

