using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using WF.Shared.Contracts.Abstractions;
using WF.Shared.Contracts.Dtos;

namespace WF.TransactionService.Infrastructure.HttpClients;

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

    public async Task<CustomerVerificationDto?> GetVerificationDataAsync(Guid customerId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpClient.GetAsync($"api/v1/customers/{customerId}/verification-data", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var verificationData = await response.Content.ReadFromJsonAsync<CustomerVerificationDto>(cancellationToken: cancellationToken);
                logger.LogInformation("Successfully retrieved customer verification data {CustomerId}", customerId);
                return verificationData;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                logger.LogWarning("Customer verification data {CustomerId} not found", customerId);
                return null;
            }

            response.EnsureSuccessStatusCode();
            return null;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Error occurred while retrieving customer verification data {CustomerId}", customerId);
            throw;
        }
    }

    public async Task<Guid?> GetCustomerIdByCustomerNumberAsync(string customerNumber, CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpClient.GetAsync($"api/customers/number/{customerNumber}/id", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var customerId = await response.Content.ReadFromJsonAsync<Guid?>(cancellationToken: cancellationToken);
                logger.LogInformation("Successfully retrieved customer ID for customer number {CustomerNumber}", customerNumber);
                return customerId;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                logger.LogWarning("Customer with number {CustomerNumber} not found", customerNumber);
                return null;
            }

            response.EnsureSuccessStatusCode();
            return null;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Error occurred while retrieving customer ID for customer number {CustomerNumber}", customerNumber);
            throw;
        }
    }

    public async Task<List<CustomerLookupDto>> LookupByCustomerNumbersAsync(List<string> customerNumbers, CancellationToken cancellationToken)
    {
        try
        {
            var requestBody = new { CustomerNumbers = customerNumbers };
            var response = await httpClient.PostAsJsonAsync("api/v1/customers/lookup-by-numbers", requestBody, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var results = await response.Content.ReadFromJsonAsync<List<CustomerLookupDto>>(cancellationToken: cancellationToken);
                logger.LogInformation("Successfully retrieved customer lookups for {Count} customer numbers", customerNumbers.Count);
                return results ?? new List<CustomerLookupDto>();
            }

            response.EnsureSuccessStatusCode();
            return new List<CustomerLookupDto>();
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Error occurred while retrieving customer lookups for customer numbers");
            throw;
        }
    }
}

