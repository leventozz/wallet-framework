using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace WF.CustomerService.Application
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddApplication(
        this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            return services;
        }
    }
}
