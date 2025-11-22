using WF.FraudService.Domain.ValueObjects;

namespace WF.FraudService.Application.Contracts.DTOs;

public class RiskyHourRuleDto
{
    //let be complex for admin panel future features
    public Guid Id { get; set; }
    public int StartHour { get; set; }
    public int EndHour { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public static class RiskyHourRuleDtoExtensions
{
    public static TimeRange ToTimeRange(this RiskyHourRuleDto dto)
    {
        return TimeRange.Create(dto.StartHour, dto.EndHour);
    }

    public static bool IsCurrentTimeRisky(this RiskyHourRuleDto dto, DateTime utcDateTime)
    {
        var timeRangeResult = dto.ToTimeRange();

        return timeRangeResult.IsCurrentTimeInRange(utcDateTime);
    }
}