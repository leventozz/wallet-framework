namespace WF.FraudService.Domain.ValueObjects;

public readonly record struct TimeRange
{
    public int StartHour { get; }
    public int EndHour { get; }

    public TimeRange(int startHour, int endHour)
    {
        if (startHour < 0 || startHour > 23)
            throw new ArgumentOutOfRangeException(nameof(startHour), "Start hour must be between 0 and 23.");

        if (endHour < 0 || endHour > 23)
            throw new ArgumentOutOfRangeException(nameof(endHour), "End hour must be between 0 and 23.");

        StartHour = startHour;
        EndHour = endHour;
    }

    public static TimeRange Create(int startHour, int endHour) => new(startHour, endHour);

    public bool IsCurrentTimeInRange(DateTime utcDateTime)
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

