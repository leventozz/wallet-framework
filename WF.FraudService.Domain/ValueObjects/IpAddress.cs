using System.Net;

namespace WF.FraudService.Domain.ValueObjects;

public readonly record struct IpAddress
{
    public IPAddress Value { get; }

    public IpAddress(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("IP address cannot be null or empty.", nameof(value));

        try
        {
            Value = IPAddress.Parse(value.Trim());
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("Invalid IP address format.", nameof(value), ex);
        }
    }

    private IpAddress(IPAddress value)
    {
        Value = value;
    }

    public static implicit operator string(IpAddress ipAddress) => ipAddress.Value.ToString();
    
    public static implicit operator IpAddress(string value) => new(value);
    
    public static implicit operator IPAddress(IpAddress ipAddress) => ipAddress.Value;
    
    public static implicit operator IpAddress(IPAddress value) => new(value);

    public override string ToString() => Value.ToString();
}

