using Microsoft.Extensions.Logging;
using WF.FraudService.Application.Contracts;
using WF.FraudService.Application.Contracts.DTOs;
using WF.FraudService.Application.Features.FraudChecks.Commands.CheckFraud;
using WF.Shared.Contracts.Abstractions;
using WF.Shared.Contracts.Result;

namespace WF.FraudService.Application.Features.FraudChecks.Rules;

public class KycLevelFraudRule(
    IFraudRuleReadService _readService,
    ICustomerServiceApiClient _customerServiceApiClient,
    ILogger<KycLevelFraudRule> _logger) : IFraudEvaluationRule
{
    public int Priority => 4;

    public async Task<Result> EvaluateAsync(CheckFraudCommandInternal request, CancellationToken cancellationToken)
    {
        var kycLevelRuleDtos = await _readService.GetActiveKycLevelRulesAsync(cancellationToken);
        
        if (!kycLevelRuleDtos.Any())
        {
            return Result.Success();
        }

        var verificationData = await _customerServiceApiClient.GetVerificationDataAsync(request.SenderCustomerId, cancellationToken);
        
        if (verificationData == null)
        {
            _logger.LogWarning("Customer {CustomerId} not found, declined transaction", request.SenderCustomerId);
            return Result.Failure(Error.Failure("FraudCheck", "Customer not found"));
        }

        foreach (var dto in kycLevelRuleDtos)
        {
            if (!dto.IsAmountAllowed(request.Amount, verificationData.KycStatus))
            {
                var failureReason = $"Amount {request.Amount} exceeds maximum allowed amount {dto.MaxAllowedAmount} for KYC level rule. Customer KYC status: {verificationData.KycStatus}, Required: {dto.RequiredKycStatus}";
                return Result.Failure(Error.Failure("FraudCheck", failureReason));
            }
        }

        return Result.Success();
    }
}

