using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using WF.Shared.Contracts.Abstractions;
using WF.Shared.Contracts.Dtos;

namespace WF.TransactionService.Infrastructure.HttpClients;

public class WalletServiceApiClient(HttpClient httpClient, ILogger<WalletServiceApiClient> logger) : IWalletServiceApiClient
{
    public async Task<Guid?> GetWalletIdByCustomerIdAndCurrencyAsync(Guid customerId, string currency, CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpClient.GetAsync($"api/v1/internal/wallets/by-customer/{customerId}/currency/{currency}", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var walletId = await response.Content.ReadFromJsonAsync<Guid>(cancellationToken: cancellationToken);
                logger.LogInformation("Successfully retrieved wallet ID {WalletId} for customer {CustomerId} with currency {Currency}", walletId, customerId, currency);
                return walletId;
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
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

    public async Task<List<WalletLookupDto>> LookupByCustomerIdsAsync(List<Guid> customerIds, string currency, CancellationToken cancellationToken)
    {
        try
        {
            var requestBody = new { CustomerIds = customerIds, Currency = currency };
            var response = await httpClient.PostAsJsonAsync("api/v1/internal/wallets/lookup-by-customer-ids", requestBody, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var results = await response.Content.ReadFromJsonAsync<List<WalletLookupDto>>(cancellationToken: cancellationToken);
                logger.LogInformation("Successfully retrieved wallet lookups for {Count} customer IDs with currency {Currency}", customerIds.Count, currency);
                return results ?? new List<WalletLookupDto>();
            }
            else
            {
                logger.LogWarning("Wallet lookup failed with {StatusCode} for currency {Currency}", response.StatusCode, currency);
                return new List<WalletLookupDto>();
            }
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Error occurred while retrieving wallet lookups for customer IDs with currency {Currency}", currency);
            throw;
        }
    }
}

