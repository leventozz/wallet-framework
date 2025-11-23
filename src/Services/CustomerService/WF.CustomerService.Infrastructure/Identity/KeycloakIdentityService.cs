using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WF.CustomerService.Application.Abstractions.Identity;

namespace WF.CustomerService.Infrastructure.Identity;

public class KeycloakIdentityService : IIdentityService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly KeycloakOptions _options;
    private readonly ILogger<KeycloakIdentityService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public KeycloakIdentityService(
        IHttpClientFactory httpClientFactory,
        IOptions<KeycloakOptions> options,
        ILogger<KeycloakIdentityService> logger)
    {
        _httpClientFactory = httpClientFactory;
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

            var (userId, isNewUser) = await CreateUserAsync(accessToken, email, password, firstName, lastName, cancellationToken);

            if (isNewUser)
            {
                try
                {
                    await SendVerifyEmailAsync(accessToken, userId, cancellationToken);
                    _logger.LogInformation(
                        "Verification email sent successfully for user with email {Email} and userId {UserId}",
                        email,
                        userId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Failed to send verification email for user with email {Email} and userId {UserId}. User registration was successful.",
                        email,
                        userId);
                }
            }

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
        using var httpClient = _httpClientFactory.CreateClient(nameof(IIdentityService));
        
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

        var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(
            JsonOptions,
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Failed to deserialize token response from Keycloak.");

        return tokenResponse.AccessToken;
    }

    private async Task<(string userId, bool isNewUser)> CreateUserAsync(
        string accessToken,
        string email,
        string password,
        string firstName,
        string lastName,
        CancellationToken cancellationToken)
    {
        using var httpClient = _httpClientFactory.CreateClient(nameof(IIdentityService));
        
        var createUserEndpoint = $"{_options.BaseUrl}/admin/realms/{_options.Realm}/users";

        var userRequest = new
        {
            username = email,
            email = email,
            firstName = firstName,
            lastName = lastName,
            enabled = true,
            emailVerified = false,
            RequiredActions = new List<string> { "VERIFY_EMAIL" },
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

        var response = await httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            _logger.LogWarning(
                "User with email {Email} already exists in Keycloak. Attempting to retrieve existing user.",
                email);
            var existingUserId = await GetUserIdByEmailAsync(accessToken, email, cancellationToken);
            return (existingUserId, false);
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
        string userId;
        if (string.IsNullOrEmpty(locationHeader))
        {
            userId = await GetUserIdByEmailAsync(accessToken, email, cancellationToken);
        }
        else
        {
            userId = ExtractUserIdFromLocation(locationHeader);
        }

        return (userId, true);
    }

    private async Task<string> GetUserIdByEmailAsync(
        string accessToken,
        string email,
        CancellationToken cancellationToken)
    {
        using var httpClient = _httpClientFactory.CreateClient(nameof(IIdentityService));
        
        var searchEndpoint = $"{_options.BaseUrl}/admin/realms/{_options.Realm}/users?email={Uri.EscapeDataString(email)}";

        var request = new HttpRequestMessage(HttpMethod.Get, searchEndpoint)
        {
            Headers =
            {
                { "Authorization", $"Bearer {accessToken}" }
            }
        };

        var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var users = await response.Content.ReadFromJsonAsync<List<KeycloakUser>>(
            JsonOptions,
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Failed to deserialize user search response from Keycloak.");

        var user = users.FirstOrDefault()
            ?? throw new InvalidOperationException($"User with email {email} was not found after creation.");

        return user.Id;
    }

    private async Task SendVerifyEmailAsync(
        string accessToken,
        string userId,
        CancellationToken cancellationToken)
    {
        using var httpClient = _httpClientFactory.CreateClient(nameof(IIdentityService));
        
        var sendVerifyEmailEndpoint = $"{_options.BaseUrl}/admin/realms/{_options.Realm}/users/{userId}/send-verify-email";

        var request = new HttpRequestMessage(HttpMethod.Put, sendVerifyEmailEndpoint)
        {
            Headers =
            {
                { "Authorization", $"Bearer {accessToken}" }
            }
        };

        var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private static string ExtractUserIdFromLocation(string location)
    {
        var parts = location.Split('/');
        return parts[^1];
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

    private record KeycloakUser
    {
        public string Id { get; init; } = string.Empty;
    }
}

