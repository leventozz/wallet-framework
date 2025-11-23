using WF.Shared.Contracts.Result;

namespace WF.FraudService.Domain.ValueObjects;

public readonly record struct TimeRange
{
    public int StartHour { get; }
    public int EndHour { get; }

    private TimeRange(int startHour, int endHour)
    {
        StartHour = startHour;
        EndHour = endHour;
    }

    public static Result<TimeRange> Create(int startHour, int endHour)
    {
        if (startHour < 0 || startHour > 23)
            return Result<TimeRange>.Failure(Error.Validation("TimeRange.InvalidHour", "Start hour must be between 0 and 23."));

        if (endHour < 0 || endHour > 23)
            return Result<TimeRange>.Failure(Error.Validation("TimeRange.InvalidHour", "End hour must be between 0 and 23."));

        return Result<TimeRange>.Success(new TimeRange(startHour, endHour));
    }

    // for efcore
    public static TimeRange FromDatabaseValue(int startHour, int endHour)
    {
        if (startHour < 0 || startHour > 23)
            throw new InvalidOperationException("Start hour must be between 0 and 23 when reading from database.");

        if (endHour < 0 || endHour > 23)
            throw new InvalidOperationException("End hour must be between 0 and 23 when reading from database.");

        return new TimeRange(startHour, endHour);
    }

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

