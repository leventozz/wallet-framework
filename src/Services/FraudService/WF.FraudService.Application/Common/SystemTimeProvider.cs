using WF.FraudService.Application.Contracts;

namespace WF.FraudService.Application.Common;

public class SystemTimeProvider : ITimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}

