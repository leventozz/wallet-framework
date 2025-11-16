using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using WF.Shared.Contracts.Abstractions;
using WF.Shared.Contracts.Dtos;

namespace WF.FraudService.Infrastructure.HttpClients;

public class CustomerServiceApiClient(HttpClient httpClient, ILogger<CustomerServiceApiClient> logger) : ICustomerServiceApiClient
{
    public async Task<CustomerDto?> GetCustomerByIdAsync(Guid customerId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpClient.GetAsync($"api/customers/{customerId}", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var customer = await response.Content.ReadFromJsonAsync<CustomerDto>(cancellationToken: cancellationToken);
                logger.LogInformation("Successfully retrieved customer {CustomerId}", customerId);
                return customer;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                logger.LogWarning("Customer {CustomerId} not found", customerId);
                return null;
            }

            response.EnsureSuccessStatusCode();
            return null;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Error occurred while retrieving customer {CustomerId}", customerId);
            throw;
        }
    }
}

