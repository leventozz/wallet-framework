using Microsoft.Extensions.Logging;
using WF.FraudService.Application.Contracts;
using WF.FraudService.Application.Contracts.DTOs;
using WF.FraudService.Application.Features.FraudChecks.Commands.CheckFraud;
using WF.Shared.Contracts.Abstractions;

namespace WF.FraudService.Application.Features.FraudChecks.Rules;

public class KycLevelFraudRule(
    IFraudRuleReadService _readService,
    ICustomerServiceApiClient _customerServiceApiClient,
    ILogger<KycLevelFraudRule> _logger) : IFraudEvaluationRule
{
    public int Priority => 4;

    public async Task<FraudEvaluationResult> EvaluateAsync(CheckFraudCommandInternal request, CancellationToken cancellationToken)
    {
        var kycLevelRuleDtos = await _readService.GetActiveKycLevelRulesAsync(cancellationToken);
        
        if (!kycLevelRuleDtos.Any())
        {
            return new FraudEvaluationResult { IsApproved = true };
        }

        var verificationData = await _customerServiceApiClient.GetVerificationDataAsync(request.SenderCustomerId, cancellationToken);
        
        if (verificationData == null)
        {
            _logger.LogWarning("Customer {CustomerId} not found, declined transaction", request.SenderCustomerId);
            return new FraudEvaluationResult { IsApproved = false };
        }

        foreach (var dto in kycLevelRuleDtos)
        {
            if (!dto.IsAmountAllowed(request.Amount, verificationData.KycStatus))
            {
                return new FraudEvaluationResult
                {
                    IsApproved = false,
                    FailureReason = $"Amount {request.Amount} exceeds maximum allowed amount {dto.MaxAllowedAmount} for KYC level rule. Customer KYC status: {verificationData.KycStatus}, Required: {dto.RequiredKycStatus}"
                };
            }
        }

        return new FraudEvaluationResult { IsApproved = true };
    }
}

