namespace WF.FraudService.Application.Contracts;

public interface ITimeProvider
{
    DateTime UtcNow { get; }
}

