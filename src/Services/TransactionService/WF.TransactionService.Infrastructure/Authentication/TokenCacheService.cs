using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace WF.TransactionService.Infrastructure.Authentication;

public interface ITokenCacheService
{
    Task<string> GetTokenAsync(CancellationToken cancellationToken = default);
}

public class TokenCacheService(
    IKeycloakTokenService keycloakTokenService,
    IDistributedCache distributedCache,
    ILogger<TokenCacheService> logger) : ITokenCacheService, IDisposable
{
    private const string CacheKey = "service_auth_token";
    private const int RefreshBufferSeconds = 60; 
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task<string> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        var cachedToken = await GetCachedTokenAsync(cancellationToken);
        if (cachedToken != null && !IsTokenExpiringSoon(cachedToken))
        {
            logger.LogDebug("Using cached service authentication token");
            return cachedToken.Token;
        }

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            cachedToken = await GetCachedTokenAsync(cancellationToken);
            if (cachedToken != null && !IsTokenExpiringSoon(cachedToken))
            {
                logger.LogDebug("Using cached service authentication token (after lock)");
                return cachedToken.Token;
            }

            logger.LogInformation("Refreshing service authentication token");
            var tokenResult = await keycloakTokenService.GetTokenAsync(cancellationToken);
            
            var cacheExpirySeconds = Math.Max(tokenResult.ExpiresIn - RefreshBufferSeconds, 30); // Minimum 30 seconds
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(cacheExpirySeconds)
            };

            var tokenData = new CachedToken
            {
                Token = tokenResult.AccessToken,
                CachedAt = DateTime.UtcNow,
                ExpiresIn = tokenResult.ExpiresIn
            };

            var serializedToken = JsonSerializer.Serialize(tokenData);
            await distributedCache.SetStringAsync(
                CacheKey,
                serializedToken,
                cacheOptions,
                cancellationToken);

            logger.LogDebug("Service authentication token cached successfully");
            return tokenData.Token;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<CachedToken?> GetCachedTokenAsync(CancellationToken cancellationToken)
    {
        var cachedValue = await distributedCache.GetStringAsync(CacheKey, cancellationToken);
        if (string.IsNullOrWhiteSpace(cachedValue))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<CachedToken>(cachedValue);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Failed to deserialize cached token, will refresh");
            return null;
        }
    }

    private static bool IsTokenExpiringSoon(CachedToken cachedToken)
    {
        var age = DateTime.UtcNow - cachedToken.CachedAt;
        var remainingSeconds = cachedToken.ExpiresIn - age.TotalSeconds;
        return remainingSeconds <= RefreshBufferSeconds;
    }

    private record CachedToken
    {
        public string Token { get; init; } = string.Empty;
        public DateTime CachedAt { get; init; }
        public int ExpiresIn { get; init; }
    }

    public void Dispose()
    {
        _semaphore?.Dispose();
    }
}

