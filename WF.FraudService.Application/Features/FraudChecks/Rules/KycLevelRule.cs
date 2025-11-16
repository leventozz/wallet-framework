using Microsoft.Extensions.Logging;
using WF.FraudService.Application.Contracts;
using WF.FraudService.Application.Features.FraudChecks.Commands.CheckFraud;
using WF.Shared.Contracts.Abstractions;

namespace WF.FraudService.Application.Features.FraudChecks.Rules;

public class KycLevelRule(
    IFraudRuleReadService _readService,
    ICustomerServiceApiClient _customerServiceApiClient,
    ILogger<KycLevelRule> _logger) : IFraudEvaluationRule
{
    public int Priority => 4;

    public async Task<FraudEvaluationResult> EvaluateAsync(CheckFraudCommandInternal request, CancellationToken cancellationToken)
    {
        var kycLevelRules = await _readService.GetActiveKycLevelRulesAsync(cancellationToken);
        
        if (!kycLevelRules.Any())
        {
            return new FraudEvaluationResult { IsApproved = true };
        }

        var verificationData = await _customerServiceApiClient.GetVerificationDataAsync(request.SenderCustomerId, cancellationToken);
        
        if (verificationData == null)
        {
            _logger.LogWarning("Customer {CustomerId} not found, declined transaction", request.SenderCustomerId);
            return new FraudEvaluationResult { IsApproved = false };
        }

        foreach (var rule in kycLevelRules)
        {
            if (!rule.IsAmountAllowed(request.Amount, verificationData.KycStatus))
            {
                return new FraudEvaluationResult
                {
                    IsApproved = false,
                    FailureReason = $"Amount {request.Amount} exceeds maximum allowed amount {rule.MaxAllowedAmount.Value} for KYC level rule. Customer KYC status: {verificationData.KycStatus}, Required: {rule.RequiredKycStatus}"
                };
            }
        }

        return new FraudEvaluationResult { IsApproved = true };
    }
}

