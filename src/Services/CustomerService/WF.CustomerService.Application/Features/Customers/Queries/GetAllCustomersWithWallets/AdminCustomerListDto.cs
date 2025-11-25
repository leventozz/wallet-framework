namespace WF.CustomerService.Application.Features.Customers.Queries.GetAllCustomersWithWallets;

public class AdminCustomerListDto
{
    public Guid Id { get; set; }
    public string CustomerNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<AdminWalletDto> Wallets { get; set; } = [];
}

public class AdminWalletDto
{
    public Guid WalletId { get; set; }
    public string WalletNumber { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
