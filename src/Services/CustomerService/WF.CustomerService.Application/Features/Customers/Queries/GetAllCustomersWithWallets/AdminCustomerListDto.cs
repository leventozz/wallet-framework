using WF.Shared.Contracts.IntegrationEvents.Enum;

namespace WF.CustomerService.Application.Features.Customers.Queries.GetAllCustomersWithWallets;

public class AdminCustomerListDto
{
    public Guid Id { get; set; }
    public string CustomerNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string Status => IsActive ? "Active" : "Closed";
    public DateTime CreatedAt { get; set; }
    public List<AdminWalletDto> Wallets { get; set; } = [];
}

public class AdminWalletDto
{
    public Guid WalletId { get; set; }
    public string WalletNumber { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string Currency { get; set; } = string.Empty;
    public WalletState State { get; set; }
    public string Status => State.ToString();
}
