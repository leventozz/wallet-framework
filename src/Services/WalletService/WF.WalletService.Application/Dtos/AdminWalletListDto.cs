namespace WF.WalletService.Application.Dtos;

public record AdminWalletListDto
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public string WalletNumber { get; init; } = string.Empty;
    public string Currency { get; init; } = string.Empty;
    public decimal Balance { get; init; }
    public decimal AvailableBalance { get; init; }
    public bool IsActive { get; init; }
    public bool IsFrozen { get; init; }
    public bool IsClosed { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
    public string? Iban { get; init; }
    public string? LastTransactionId { get; init; }
    public DateTime? LastTransactionAtUtc { get; init; }

    public string Status
    {
        get
        {
            if (IsClosed) return "Closed";
            if (IsFrozen) return "Frozen";
            if (IsActive) return "Active";
            return "Inactive";
        }
    }
}
