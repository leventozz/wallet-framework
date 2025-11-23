namespace WF.Shared.Contracts.Configuration;

public sealed record CustomerServiceOptions
{
    public required string BaseUrl { get; init; }
}

