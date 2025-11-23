namespace WF.FraudService.Application.Contracts.DTOs;

public class BlockedIpRuleDto
{
    public Guid Id { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
    public bool IsActive { get; set; }
}

public static class BlockedIpRuleDtoExtensions
{
    public static bool IsExpired(this BlockedIpRuleDto dto, DateTime utcNow)
    {
        return dto.ExpiresAtUtc.HasValue && dto.ExpiresAtUtc.Value <= utcNow;
    }

    public static bool IsBlocked(this BlockedIpRuleDto? dto, DateTime utcNow)
    {
        return dto != null && dto.IsActive && !dto.IsExpired(utcNow);
    }
}
