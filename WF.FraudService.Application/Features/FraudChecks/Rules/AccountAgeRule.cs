using Microsoft.Extensions.Logging;
using WF.FraudService.Application.Contracts;
using WF.FraudService.Application.Features.FraudChecks.Commands.CheckFraud;
using WF.FraudService.Domain.Entities;
using WF.Shared.Contracts.Abstractions;

namespace WF.FraudService.Application.Features.FraudChecks.Rules;

public class AccountAgeRule(
    IFraudRuleReadService _readService,
    ICustomerServiceApiClient _customerServiceApiClient,
    ILogger<AccountAgeRule> _logger) : IFraudEvaluationRule
{
    public int Priority => 3;

    public async Task<FraudEvaluationResult> EvaluateAsync(CheckFraudCommandInternal request, CancellationToken cancellationToken)
    {
        var accountAgeRules = await _readService.GetActiveAccountAgeRulesAsync(cancellationToken);
        
        if (!accountAgeRules.Any())
        {
            return new FraudEvaluationResult { IsApproved = true };
        }

        var verificationData = await _customerServiceApiClient.GetVerificationDataAsync(request.SenderCustomerId, cancellationToken);
        
        if (verificationData == null)
        {
            _logger.LogWarning("Customer {CustomerId} not found, declined transaction", request.SenderCustomerId);
            return new FraudEvaluationResult { IsApproved = false };
        }

        foreach (var rule in accountAgeRules)
        {
            if (!rule.IsAmountAllowed(request.Amount, verificationData.CreatedAtUtc))
            {
                return new FraudEvaluationResult
                {
                    IsApproved = false,
                    FailureReason = $"Amount {request.Amount} exceeds maximum allowed amount {rule.MaxAllowedAmount.Value} for account age rule. Account age: {(DateTime.UtcNow - verificationData.CreatedAtUtc).Days} days, Required: {rule.MinAccountAgeDays} days"
                };
            }
        }

        return new FraudEvaluationResult { IsApproved = true };
    }
}

