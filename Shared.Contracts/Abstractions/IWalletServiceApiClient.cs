namespace WF.Shared.Contracts.Abstractions;

public interface IWalletServiceApiClient
{
    Task<Guid?> GetWalletIdByCustomerIdAndCurrencyAsync(Guid customerId, string currency, CancellationToken cancellationToken);
}

