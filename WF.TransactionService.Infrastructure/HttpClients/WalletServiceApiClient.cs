using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using WF.Shared.Contracts.Abstractions;

namespace WF.TransactionService.Infrastructure.HttpClients;

public class WalletServiceApiClient(HttpClient httpClient, ILogger<WalletServiceApiClient> logger) : IWalletServiceApiClient
{
    public async Task<Guid?> GetWalletIdByCustomerIdAndCurrencyAsync(Guid customerId, string currency, CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpClient.GetAsync($"api/wallets/by-customer/{customerId}/currency/{currency}", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var walletId = await response.Content.ReadFromJsonAsync<Guid>(cancellationToken: cancellationToken);
                logger.LogInformation("Successfully retrieved wallet ID {WalletId} for customer {CustomerId} with currency {Currency}", walletId, customerId, currency);
                return walletId;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                logger.LogWarning("Wallet not found for customer {CustomerId} with currency {Currency}", customerId, currency);
                return null;
            }

            response.EnsureSuccessStatusCode();
            return null;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Error occurred while retrieving wallet ID for customer {CustomerId} with currency {Currency}", customerId, currency);
            throw;
        }
    }
}

