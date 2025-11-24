using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using WF.Shared.Contracts.Abstractions;
using WF.Shared.Contracts.Configuration;
using WF.TransactionService.Application.Abstractions;
using WF.TransactionService.Application.Contracts;
using WF.TransactionService.Infrastructure.EventBus;
using WF.TransactionService.Domain.Abstractions;
using WF.TransactionService.Domain.Entities;
using WF.TransactionService.Infrastructure.Data;
using WF.TransactionService.Infrastructure.Features.Sagas;
using WF.TransactionService.Infrastructure.MachineContext;
using WF.TransactionService.Infrastructure.QueryServices;
using WF.TransactionService.Infrastructure.Repositories;
using WF.TransactionService.Infrastructure.HttpClients;
using WF.TransactionService.Infrastructure.Authentication;
using WF.TransactionService.Infrastructure.Data.Interceptors;

namespace WF.TransactionService.Infrastructure
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");


            services.AddScoped<AuditableEntityInterceptor>();

            services.AddDbContext<TransactionDbContext>((serviceProvider, options) =>
            {
                options.UseNpgsql(connectionString);
                options.AddInterceptors(serviceProvider.GetRequiredService<AuditableEntityInterceptor>());
            });

            services.AddNpgsqlDataSource(connectionString);

            services.AddSingleton<IMachineContextProvider>(serviceProvider =>
            {
                var env = serviceProvider.GetRequiredService<IHostEnvironment>();
                return new EnvironmentMachineContextProvider(env, configuration);
            });

            services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMQ"));
            services.Configure<RedisOptions>(configuration.GetSection("Redis"));
            services.Configure<KeycloakOptions>(configuration.GetSection("Keycloak"));
            services.Configure<CustomerServiceOptions>(configuration.GetSection("CustomerService"));
            services.Configure<WalletServiceOptions>(configuration.GetSection("WalletService"));
            
            services.AddStackExchangeRedisCache(options =>
            {
                var redisOptionsValue = configuration.GetSection("Redis").Get<RedisOptions>()
                    ?? throw new InvalidOperationException("Redis configuration not found.");
                options.Configuration = redisOptionsValue.GetConnectionString();
            });

            services.AddHttpClient("Keycloak", (serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<KeycloakOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            services.AddSingleton<IKeycloakTokenService, KeycloakTokenService>();
            services.AddSingleton<ITokenCacheService, TokenCacheService>();
            services.AddScoped<ServiceAuthenticationHandler>();

            services.AddHttpClient<ICustomerServiceApiClient, CustomerServiceApiClient>((serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<CustomerServiceOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddHttpMessageHandler<ServiceAuthenticationHandler>();

            services.AddHttpClient<IWalletServiceApiClient, WalletServiceApiClient>((serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<WalletServiceOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddHttpMessageHandler<ServiceAuthenticationHandler>();

            services.AddMassTransit(mtConfig =>
            {
                mtConfig.AddSagaStateMachine<TransferSagaStateMachine, Transaction>()
                    .EntityFrameworkRepository(r =>
                    {
                        r.ExistingDbContext<TransactionDbContext>();
                        r.LockStatementProvider = new PostgresLockStatementProvider();
                    });

                mtConfig.AddEntityFrameworkOutbox<TransactionDbContext>(o =>
                {
                    o.UsePostgres();
                    o.DuplicateDetectionWindow = TimeSpan.FromSeconds(30);
                });

                mtConfig.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitMqOptions = context.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
                    
                    cfg.Host(rabbitMqOptions.Host, (ushort)rabbitMqOptions.Port, rabbitMqOptions.VirtualHost, h =>
                    {
                        h.Username(rabbitMqOptions.Username);
                        h.Password(rabbitMqOptions.Password);
                    });

                    cfg.ConfigureEndpoints(context);
                });
            });

            services.AddScoped<ITransferRequestRepository, TransferRequestRepository>();
            services.AddScoped<ITransactionReadService, TransactionQueryService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IIntegrationEventPublisher, MassTransitEventPublisher>();

            return services;
        }
    }
}

