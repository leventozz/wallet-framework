namespace WF.FraudService.Domain.Entities;

public class RiskyHourRule
{
    public Guid Id { get; private set; }
    public int StartHour { get; private set; }
    public int EndHour { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private RiskyHourRule() { }

    public RiskyHourRule(int startHour, int endHour, string? description = null)
    {
        if (startHour < 0 || startHour > 23)
            throw new ArgumentOutOfRangeException(nameof(startHour), "Start hour must be between 0 and 23.");

        if (endHour < 0 || endHour > 23)
            throw new ArgumentOutOfRangeException(nameof(endHour), "End hour must be between 0 and 23.");

        Id = Guid.NewGuid();
        StartHour = startHour;
        EndHour = endHour;
        Description = description;
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
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

    public void UpdateHours(int startHour, int endHour)
    {
        if (startHour < 0 || startHour > 23)
            throw new ArgumentOutOfRangeException(nameof(startHour), "Start hour must be between 0 and 23.");

        if (endHour < 0 || endHour > 23)
            throw new ArgumentOutOfRangeException(nameof(endHour), "End hour must be between 0 and 23.");

        StartHour = startHour;
        EndHour = endHour;
    }

    public bool IsInRiskyHour(DateTime utcDateTime)
    {
        var currentHour = utcDateTime.Hour;
        
        if (StartHour <= EndHour)
        {
            return currentHour >= StartHour && currentHour <= EndHour;
        }
        else
        {
            return currentHour >= StartHour || currentHour <= EndHour;
        }
    }
}

