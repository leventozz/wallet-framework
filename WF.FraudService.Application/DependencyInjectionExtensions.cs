using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using WF.FraudService.Application.Contracts;
using WF.FraudService.Application.Features.FraudChecks.Rules;
using WF.Shared.Application;

namespace WF.FraudService.Application;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddScoped<IFraudEvaluationRule, BlockedIpRule>();
        services.AddScoped<IFraudEvaluationRule, RiskyHourRule>();
        services.AddScoped<IFraudEvaluationRule, AccountAgeRule>();
        services.AddScoped<IFraudEvaluationRule, KycLevelRule>();

        return services;
    }
}

