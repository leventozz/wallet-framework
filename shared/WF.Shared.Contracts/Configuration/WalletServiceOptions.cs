namespace WF.Shared.Contracts.Configuration;

public sealed record WalletServiceOptions
{
    public required string BaseUrl { get; init; }
}

