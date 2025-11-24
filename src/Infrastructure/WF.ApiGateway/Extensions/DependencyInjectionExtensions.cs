using AspNetCoreRateLimit;
using AspNetCoreRateLimit.Redis;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using WF.Shared.Contracts.Configuration;

namespace WF.ApiGateway.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddRedis(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RedisOptions>(configuration.GetSection("Redis"));
        
        var redisOptions = configuration.GetSection("Redis").Get<RedisOptions>()
            ?? throw new InvalidOperationException("Redis configuration not found.");

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisOptions.GetConnectionString();
        });

        var redisConnection = ConnectionMultiplexer.Connect(redisOptions.GetConnectionString());
        services.AddSingleton<IConnectionMultiplexer>(redisConnection);

        return services;
    }

    public static IServiceCollection AddRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions();
        services.AddMemoryCache();
        
        services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
        services.Configure<IpRateLimitPolicies>(configuration.GetSection("IpRateLimitPolicies"));
        
        services.AddSingleton<IIpPolicyStore, DistributedCacheIpPolicyStore>();
        services.AddSingleton<IRateLimitCounterStore, DistributedCacheRateLimitCounterStore>();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        services.AddSingleton<IProcessingStrategy, RedisProcessingStrategy>();

        return services;
    }
}

