using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WF.FraudService.Application.Contracts;
using WF.FraudService.Domain.Abstractions;
using WF.FraudService.Infrastructure.Data;
using WF.FraudService.Infrastructure.QueryServices;
using WF.FraudService.Infrastructure.Repositories;
using WF.Shared.Abstractions;
using WF.Shared.Infrastructure.Configuration;
using WF.Shared.Infrastructure.EventBus;

namespace WF.FraudService.Infrastructure;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<FraudDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddNpgsqlDataSource(connectionString);

        services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMQ"));

        services.AddMassTransit(mtConfig =>
        {
            mtConfig.AddEntityFrameworkOutbox<FraudDbContext>(o =>
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

        services.AddScoped<IBlockedIpRepository, BlockedIpRuleRepository>();
        services.AddScoped<IRiskyHourRuleRepository, RiskyHourRuleRepository>();
        services.AddScoped<IAccountAgeRuleRepository, AccountAgeRuleRepository>();
        services.AddScoped<IKycLevelRuleRepository, KycLevelRuleRepository>();
        services.AddScoped<IFraudRuleReadService, FraudRuleReadService>();
        services.AddScoped<IIntegrationEventPublisher, MassTransitEventPublisher>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}

