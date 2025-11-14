using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WF.Shared.Abstractions;
using WF.Shared.Infrastructure.Configuration;
using WF.Shared.Infrastructure.EventBus;
using WF.TransactionService.Application.Contracts;
using WF.TransactionService.Domain.Entities;
using WF.TransactionService.Domain.Repositories;
using WF.TransactionService.Infrastructure.Data;
using WF.TransactionService.Infrastructure.Features.Sagas;
using WF.TransactionService.Infrastructure.QueryServices;
using WF.TransactionService.Infrastructure.Repositories;

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

            services.AddDbContext<TransactionDbContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddNpgsqlDataSource(connectionString);

            services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMQ"));

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

