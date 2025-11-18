namespace WF.Shared.Contracts.Dtos;

public record CustomerLookupDto
{
    public Guid CustomerId { get; init; }
    public string CustomerNumber { get; init; } = string.Empty;
}

