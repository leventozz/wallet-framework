using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WF.TransactionService.Application.Abstractions;
using WF.TransactionService.Application.Contracts;
using WF.TransactionService.Domain.Repositories;
using WF.TransactionService.Infrastructure.Data;
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

            services.AddScoped<ITransferRequestRepository, TransferRequestRepository>();
            services.AddScoped<ITransactionReadService, TransactionQueryService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}

