using WF.FraudService.Domain.ValueObjects;

namespace WF.FraudService.Domain.Entities;

public class RiskyHourRule
{
    public Guid Id { get; private set; }
    public TimeRange TimeRange { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private RiskyHourRule() { }

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

