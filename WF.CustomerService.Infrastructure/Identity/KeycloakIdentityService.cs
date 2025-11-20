using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WF.CustomerService.Application.Abstractions.Identity;

namespace WF.CustomerService.Infrastructure.Identity;

public class KeycloakIdentityService : IIdentityService
{
    private readonly HttpClient _httpClient;
    private readonly KeycloakOptions _options;
    private readonly ILogger<KeycloakIdentityService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public KeycloakIdentityService(
        HttpClient httpClient,
        IOptions<KeycloakOptions> options,
        ILogger<KeycloakIdentityService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> RegisterUserAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        CancellationToken cancellationToken)
    {
        try
        {
            var accessToken = await GetAdminTokenAsync(cancellationToken);

            var userId = await CreateUserAsync(accessToken, email, password, firstName, lastName, cancellationToken);

            _logger.LogInformation(
                "User registered successfully in Keycloak with email {Email} and userId {UserId}",
                email,
                userId);

            return userId;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to register user in Keycloak for email {Email}", email);
            throw new InvalidOperationException("Failed to register user in identity provider.", ex);
        }
    }

    private async Task<string> GetAdminTokenAsync(CancellationToken cancellationToken)
    {
        var tokenEndpoint = $"{_options.BaseUrl}/realms/{_options.Realm}/protocol/openid-connect/token";

        var requestBody = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", _options.ClientId },
            { "client_secret", _options.ClientSecret }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
        {
            Content = new FormUrlEncodedContent(requestBody)
        };

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(
            JsonOptions,
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Failed to deserialize token response from Keycloak.");

        return tokenResponse.AccessToken;
    }

    private async Task<string> CreateUserAsync(
        string accessToken,
        string email,
        string password,
        string firstName,
        string lastName,
        CancellationToken cancellationToken)
    {
        var createUserEndpoint = $"{_options.BaseUrl}/admin/realms/{_options.Realm}/users";

        var userRequest = new
        {
            email = email,
            firstName = firstName,
            lastName = lastName,
            enabled = true,
            emailVerified = false,
            credentials = new[]
            {
                new
                {
                    type = "password",
                    value = password,
                    temporary = false
                }
            }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, createUserEndpoint)
        {
            Content = JsonContent.Create(userRequest, options: JsonOptions),
            Headers =
            {
                { "Authorization", $"Bearer {accessToken}" }
            }
        };

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            _logger.LogWarning(
                "User with email {Email} already exists in Keycloak. Attempting to retrieve existing user.",
                email);
            var existingUserId = await GetUserIdByEmailAsync(accessToken, email, cancellationToken);
            return existingUserId;
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "Failed to create user in Keycloak. Status: {StatusCode}, Response: {ErrorContent}",
                response.StatusCode,
                errorContent);
            throw new HttpRequestException(
                $"Failed to create user in Keycloak. Status: {response.StatusCode}, Response: {errorContent}");
        }

        var locationHeader = response.Headers.Location?.ToString();
        if (string.IsNullOrEmpty(locationHeader))
        {
            var userId = await GetUserIdByEmailAsync(accessToken, email, cancellationToken);
            return userId;
        }

        var userIdFromLocation = ExtractUserIdFromLocation(locationHeader);
        return userIdFromLocation;
    }

    private async Task<string> GetUserIdByEmailAsync(
        string accessToken,
        string email,
        CancellationToken cancellationToken)
    {
        var searchEndpoint = $"{_options.BaseUrl}/admin/realms/{_options.Realm}/users?email={Uri.EscapeDataString(email)}";

        var request = new HttpRequestMessage(HttpMethod.Get, searchEndpoint)
        {
            Headers =
            {
                { "Authorization", $"Bearer {accessToken}" }
            }
        };

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var users = await response.Content.ReadFromJsonAsync<List<KeycloakUser>>(
            JsonOptions,
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Failed to deserialize user search response from Keycloak.");

        var user = users.FirstOrDefault()
            ?? throw new InvalidOperationException($"User with email {email} was not found after creation.");

        return user.Id;
    }

    private static string ExtractUserIdFromLocation(string location)
    {
        var parts = location.Split('/');
        return parts[^1];
    }

    private record TokenResponse
    {
        public string AccessToken { get; init; } = string.Empty;
    }

    private record KeycloakUser
    {
        public string Id { get; init; } = string.Empty;
    }
}

