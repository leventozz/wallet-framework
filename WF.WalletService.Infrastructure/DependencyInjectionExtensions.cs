using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WF.Shared.Contracts.Abstractions;
using WF.Shared.Contracts.Configuration;
using WF.WalletService.Application.Abstractions;
using WF.WalletService.Infrastructure.EventBus;
using WF.WalletService.Domain.Abstractions;
using WF.WalletService.Infrastructure.Consumers;
using WF.WalletService.Infrastructure.Data;
using WF.WalletService.Infrastructure.QueryServices;
using WF.WalletService.Infrastructure.Repositories;

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

            services.AddDbContext<WalletDbContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddNpgsqlDataSource(connectionString);

            services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMQ"));

            services.AddMassTransit(mtConfig =>
            {
                mtConfig.AddEntityFrameworkOutbox<WalletDbContext>(o =>
                {
                    o.UsePostgres();
                    o.DuplicateDetectionWindow = TimeSpan.FromSeconds(30);
                });

                mtConfig.AddConsumer<CustomerCreatedConsumer>();
                mtConfig.AddConsumer<DebitSenderWalletCommandConsumer>();
                mtConfig.AddConsumer<CreditWalletCommandConsumer>();

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

            services.AddScoped<IWalletRepository, WalletRepository>();
            services.AddScoped<IWalletQueryService, WalletQueryService>();
            services.AddScoped<IIntegrationEventPublisher, MassTransitEventPublisher>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}

