using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;
using WF.Shared.Contracts.Abstractions;
using WF.Shared.Contracts.Configuration;
using WF.WalletService.Application.Abstractions;
using WF.WalletService.Infrastructure.EventBus;
using WF.WalletService.Domain.Abstractions;
using WF.WalletService.Infrastructure.Consumers;
using WF.WalletService.Infrastructure.Data;
using WF.WalletService.Infrastructure.QueryServices;
using WF.WalletService.Infrastructure.Repositories;
using WF.WalletService.Infrastructure.PropagationContext;
using WF.WalletService.Infrastructure.MassTransit.Filters;
using WF.WalletService.Infrastructure.Data.Interceptors;

namespace WF.WalletService.Infrastructure
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            services.AddSingleton<UserContext>();

            services.AddScoped<AuditableEntityInterceptor>();

            services.AddDbContext<WalletDbContext>((serviceProvider, options) =>
            {
                options.UseNpgsql(connectionString);
                options.AddInterceptors(serviceProvider.GetRequiredService<AuditableEntityInterceptor>());
            });

            services.AddNpgsqlDataSource(connectionString);

            services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMQ"));

            services.AddMassTransit(mtConfig =>
            {
                mtConfig.AddEntityFrameworkOutbox<WalletDbContext>(o =>
                {
                    o.UsePostgres();
                    o.DuplicateDetectionWindow = TimeSpan.FromSeconds(30);
                    o.UseBusOutbox();
                });

                mtConfig.AddConsumer<CustomerCreatedConsumer>();
                mtConfig.AddConsumer<DebitSenderWalletCommandConsumer>();
                mtConfig.AddConsumer<CreditWalletCommandConsumer>();
                mtConfig.AddConsumer<RefundSenderWalletCommandConsumer>();

                mtConfig.AddDelayedMessageScheduler();

                mtConfig.AddConfigureEndpointsCallback((context, name, cfg) =>
                {
                    cfg.UseMessageRetry(r =>
                    {
                        r.Exponential(
                            retryLimit: 5,
                            minInterval: TimeSpan.FromMilliseconds(100),
                            maxInterval: TimeSpan.FromMilliseconds(1600),
                            intervalDelta: TimeSpan.FromMilliseconds(200));
                        
                        r.Handle<DbUpdateConcurrencyException>();
                        r.Handle<PostgresException>(x => x.SqlState == "40001");
                    });
                });

                mtConfig.UsingRabbitMq((context, cfg) =>
                {
                    cfg.UseDelayedMessageScheduler();

                    var rabbitMqOptions = context.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
                    
                    cfg.Host(rabbitMqOptions.Host, (ushort)rabbitMqOptions.Port, rabbitMqOptions.VirtualHost, h =>
                    {
                        h.Username(rabbitMqOptions.Username);
                        h.Password(rabbitMqOptions.Password);
                    });

                    cfg.UseConsumeFilter(typeof(ExtractUserIdConsumeFilter<>), context);

                    cfg.ConfigureEndpoints(context);
                });
            });

            services.AddScoped<IWalletRepository, WalletRepository>();
            services.AddScoped<IWalletQueryService, WalletQueryService>();
            services.AddScoped<IAdminWalletQueryService, AdminWalletQueryService>();
            services.AddScoped<IIntegrationEventPublisher, MassTransitEventPublisher>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}

