using WF.FraudService.Domain.ValueObjects;
using WF.Shared.Contracts.Result;

namespace WF.FraudService.Domain.Entities;

public class RiskyHourRule
{
    public Guid Id { get; private set; }
    public TimeRange TimeRange { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private RiskyHourRule() { }

    public static Result<RiskyHourRule> Create(TimeRange timeRange, string? description)
    {
        return Result<RiskyHourRule>.Success(new RiskyHourRule
        {
            Id = Guid.NewGuid(),
            TimeRange = timeRange,
            Description = description,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        });
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
    }

    public void UpdateHours(TimeRange timeRange)
    {
        TimeRange = timeRange;
    }
}

