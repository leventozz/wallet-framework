using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace WF.TransactionService.Middleware
{
    public static class ExceptionHandlerDIExtensions
    {
        public static IApplicationBuilder UseWFExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler();
            return app;
        }

        public static IServiceCollection AddWFExceptionHandler(this IServiceCollection services)
        {
            services.AddExceptionHandler<ExceptionHandler>();
            services.AddProblemDetails();
            return services;
        }
    }
}

