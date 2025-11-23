using WF.Shared.Contracts.IntegrationEvents.Enum;

namespace WF.Shared.Contracts.Dtos
{
    public record WalletSummaryDto
    {
        public Guid WalletId { get; set; }
        public decimal Balance { get; set; }
        public string Currency { get; set; } = string.Empty;
        public WalletState State { get; set; }
    }
}
