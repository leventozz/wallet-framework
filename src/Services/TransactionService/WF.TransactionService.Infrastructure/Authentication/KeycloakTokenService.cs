using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WF.Shared.Contracts.Configuration;

namespace WF.TransactionService.Infrastructure.Authentication;


public class KeycloakTokenService(
    IHttpClientFactory httpClientFactory,
    IOptions<KeycloakOptions> options,
    ILogger<KeycloakTokenService> logger) : IKeycloakTokenService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var result = await GetTokenAsync(cancellationToken);
        return result.AccessToken;
    }

    public async Task<TokenResult> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        using var httpClient = httpClientFactory.CreateClient("Keycloak");
        
        var tokenEndpoint = $"{options.Value.BaseUrl}/realms/{options.Value.Realm}/protocol/openid-connect/token";

        var requestBody = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", options.Value.ClientId },
            { "client_secret", options.Value.ClientSecret }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
        {
            Content = new FormUrlEncodedContent(requestBody)
        };

        try
        {
            var response = await httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(
                JsonOptions,
                cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException("Failed to deserialize token response from Keycloak.");

            logger.LogDebug("Successfully obtained access token from Keycloak. Expires in {ExpiresIn} seconds", tokenResponse.ExpiresIn);

            return new TokenResult
            {
                AccessToken = tokenResponse.AccessToken,
                ExpiresIn = tokenResponse.ExpiresIn
            };
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to obtain access token from Keycloak");
            throw new InvalidOperationException("Failed to obtain access token from Keycloak.", ex);
        }
    }

    private record TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; init; } = string.Empty;
        
        [JsonPropertyName("token_type")]
        public string TokenType { get; init; } = string.Empty;
        
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; init; }
    }
}

public record TokenResult
{
    public string AccessToken { get; init; } = string.Empty;
    public int ExpiresIn { get; init; }
}

