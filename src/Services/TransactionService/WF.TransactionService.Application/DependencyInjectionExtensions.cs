using FluentValidation;
using IdGen;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using WF.TransactionService.Application.Common.Behaviors;

namespace WF.TransactionService.Application;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddSingleton<IdGenerator>(sp =>
        {
            var env = sp.GetRequiredService<IHostEnvironment>();
            int machineId = 0;

            if (env.IsProduction())
            {
                var machineIdEnv = Environment.GetEnvironmentVariable("MACHINE_ID");
                if (string.IsNullOrWhiteSpace(machineIdEnv) || !int.TryParse(machineIdEnv, out machineId))
                {
                    throw new InvalidOperationException("MACHINE_ID environment variable is required in Production environment and must be a valid integer.");
                }
            }

            var options = new IdGeneratorOptions(idStructure: new IdStructure(45, 2, 16), timeSource: new DefaultTimeSource(new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
            return new IdGenerator(machineId, options);
        });

        return services;
    }
}

