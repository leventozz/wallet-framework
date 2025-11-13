using WF.Shared.Contracts.IntegrationEvents.Enum;

namespace WF.CustomerService.Infrastructure.Data.ReadModels
{
    public class WalletReadModel
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public decimal Balance { get; set; }
        //TODO: Use Currency type or use string for state
        public string Currency { get; set; } = string.Empty;
        public WalletState State{ get; set; }
    }
}
