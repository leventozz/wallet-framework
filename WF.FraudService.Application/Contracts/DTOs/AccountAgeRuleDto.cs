namespace WF.FraudService.Application.Contracts.DTOs;

public class AccountAgeRuleDto
{
    public Guid Id { get; set; }
    public int MinAccountAgeDays { get; set; }
    public decimal? MaxAllowedAmount { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public static class AccountAgeRuleDtoExtensions
{
    public static bool IsAmountAllowed(this AccountAgeRuleDto dto, decimal amount, int accountAgeDays)
    {
        if (!dto.MaxAllowedAmount.HasValue)
            return true;

        if (accountAgeDays < dto.MinAccountAgeDays)
            return amount <= dto.MaxAllowedAmount.Value;

        return true;
    }
}
