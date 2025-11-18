using WF.Shared.Contracts.Dtos;

namespace WF.Shared.Contracts.Abstractions;

public interface IWalletServiceApiClient
{
    Task<Guid?> GetWalletIdByCustomerIdAndCurrencyAsync(Guid customerId, string currency, CancellationToken cancellationToken);
    Task<List<WalletLookupDto>> LookupByCustomerIdsAsync(List<Guid> customerIds, string currency, CancellationToken cancellationToken);
}

