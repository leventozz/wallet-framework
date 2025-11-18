using WF.WalletService.Application.Dtos;

namespace WF.WalletService.Application.Abstractions
{
    public interface IWalletQueryService
    {
        Task<WalletDto?> GetWalletByOwnerIdAsync(Guid ownerCustomerId, CancellationToken cancellationToken);
        Task<Guid?> GetWalletIdByCustomerIdAndCurrencyAsync(Guid customerId, string currency, CancellationToken cancellationToken);
    }
}
