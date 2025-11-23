namespace WF.Shared.Contracts.Configuration;

public sealed record RedisOptions
{
    public string? ConnectionString { get; init; }
    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 6379;
    
    public string GetConnectionString()
    {
        if (!string.IsNullOrWhiteSpace(ConnectionString))
        {
            return ConnectionString;
        }
        
        return $"{Host}:{Port}";
    }
}

