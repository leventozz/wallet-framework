using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WF.CustomerService.Domain.Repositories;
using WF.CustomerService.Infrastructure.Data;
using WF.CustomerService.Infrastructure.Repositories;

namespace WF.CustomerService.Infrastructure
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
        {
            services.AddDbContext<CustomerDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.")));

            services.AddScoped<ICustomerRepository, CustomerRepository>();

            return services;
        }
    }
}
