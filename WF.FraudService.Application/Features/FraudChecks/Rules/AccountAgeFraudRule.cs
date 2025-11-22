using Microsoft.Extensions.Logging;
using WF.FraudService.Application.Contracts;
using WF.FraudService.Application.Contracts.DTOs;
using WF.FraudService.Application.Features.FraudChecks.Commands.CheckFraud;
using WF.Shared.Contracts.Abstractions;

namespace WF.FraudService.Application.Features.FraudChecks.Rules;

public class AccountAgeFraudRule(
    IFraudRuleReadService _readService,
    ICustomerServiceApiClient _customerServiceApiClient,
    ILogger<AccountAgeFraudRule> _logger) : IFraudEvaluationRule
{
    public int Priority => 3;

    public async Task<FraudEvaluationResult> EvaluateAsync(CheckFraudCommandInternal request, CancellationToken cancellationToken)
    {
        var accountAgeRuleDtos = await _readService.GetActiveAccountAgeRulesAsync(cancellationToken);
        
        if (!accountAgeRuleDtos.Any())
        {
            return new FraudEvaluationResult { IsApproved = true };
        }

        var verificationData = await _customerServiceApiClient.GetVerificationDataAsync(request.SenderCustomerId, cancellationToken);
        
        if (verificationData == null)
        {
            _logger.LogWarning("Customer {CustomerId} not found, declined transaction", request.SenderCustomerId);
            return new FraudEvaluationResult { IsApproved = false };
        }

        var accountAgeDays = (DateTime.UtcNow - verificationData.CreatedAtUtc).Days;

        foreach (var dto in accountAgeRuleDtos)
        {
            if (!dto.IsAmountAllowed(request.Amount, accountAgeDays))
            {
                return new FraudEvaluationResult
                {
                    IsApproved = false,
                    FailureReason = $"Amount {request.Amount} exceeds maximum allowed amount {dto.MaxAllowedAmount} for account age rule. Account age: {accountAgeDays} days, Required: {dto.MinAccountAgeDays} days"
                };
            }
        }

        return new FraudEvaluationResult { IsApproved = true };
    }
}

