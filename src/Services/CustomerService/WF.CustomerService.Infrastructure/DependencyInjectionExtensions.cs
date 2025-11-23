using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WF.CustomerService.Application.Abstractions;
using WF.CustomerService.Application.Abstractions.Identity;
using WF.CustomerService.Domain.Abstractions;
using WF.CustomerService.Infrastructure.Consumers;
using WF.CustomerService.Infrastructure.Data;
using WF.CustomerService.Infrastructure.Identity;
using WF.CustomerService.Infrastructure.QueryServices;
using WF.CustomerService.Infrastructure.Repositories;
using WF.CustomerService.Infrastructure.EventBus;
using WF.Shared.Contracts.Abstractions;
using WF.Shared.Contracts.Configuration;

namespace WF.CustomerService.Infrastructure
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            services.AddDbContext<CustomerDbContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddNpgsqlDataSource(connectionString);

            services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMQ"));
            services.Configure<KeycloakOptions>(configuration.GetSection("Keycloak"));

            services.AddHttpClient(nameof(IIdentityService), (serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<KeycloakOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            services.AddScoped<IIdentityService, KeycloakIdentityService>();

            services.AddMassTransit(mtConfig =>
            {
                mtConfig.AddConsumer<WalletEventsConsumer>();

                mtConfig.AddEntityFrameworkOutbox<CustomerDbContext>(o =>
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

            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<ICustomerQueryService, CustomerQueryService>();
            services.AddScoped<IIntegrationEventPublisher, MassTransitEventPublisher>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
