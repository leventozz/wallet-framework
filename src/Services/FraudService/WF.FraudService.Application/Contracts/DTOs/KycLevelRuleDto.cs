using WF.Shared.Contracts.Enums;

namespace WF.FraudService.Application.Contracts.DTOs;

public class KycLevelRuleDto
{
    public Guid Id { get; set; }
    public KycStatus RequiredKycStatus { get; set; }
    public decimal? MaxAllowedAmount { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public static class KycLevelRuleDtoExtensions
{
    public static bool IsAmountAllowed(this KycLevelRuleDto dto, decimal amount, KycStatus customerKycStatus)
    {
        if (!dto.MaxAllowedAmount.HasValue)
            return true;

        if (customerKycStatus > dto.RequiredKycStatus)
            return true;

        return amount <= dto.MaxAllowedAmount.Value;
    }
}
