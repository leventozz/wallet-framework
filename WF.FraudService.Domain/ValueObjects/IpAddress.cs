using System.Net;
using WF.Shared.Contracts.Result;

namespace WF.FraudService.Domain.ValueObjects;

public readonly record struct IpAddress
{
    public IPAddress Value { get; }

    private IpAddress(IPAddress value)
    {
        Value = value;
    }

    public static Result<IpAddress> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<IpAddress>.Failure(Error.Validation("IpAddress.Required", "IP address cannot be null or empty."));

        var trimmedValue = value.Trim();

        if (!IPAddress.TryParse(trimmedValue, out var ipAddress))
            return Result<IpAddress>.Failure(Error.Validation("IpAddress.InvalidFormat", "Invalid IP address format."));

        return Result<IpAddress>.Success(new IpAddress(ipAddress));
    }

    // for efcore
    public static IpAddress FromDatabaseValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException("IP address cannot be null or empty when reading from database.");

        var trimmedValue = value.Trim();

        if (!IPAddress.TryParse(trimmedValue, out var ipAddress))
            throw new InvalidOperationException("Invalid IP address format when reading from database.");

        return new IpAddress(ipAddress);
    }

    public static implicit operator string(IpAddress ipAddress) => ipAddress.Value.ToString();
    
    public static implicit operator IPAddress(IpAddress ipAddress) => ipAddress.Value;

    public override string ToString() => Value.ToString();
}

