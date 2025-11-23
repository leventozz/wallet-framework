namespace WF.Shared.Contracts.Dtos;

public record WalletLookupDto
{
    public Guid CustomerId { get; init; }
    public Guid WalletId { get; init; }
}

