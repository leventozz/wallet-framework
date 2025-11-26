using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using WF.FraudService.Application.Common;
using WF.FraudService.Application.Common.Behaviors;
using WF.FraudService.Application.Contracts;
using WF.FraudService.Application.Features.FraudChecks.Rules;

namespace WF.FraudService.Application;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddSingleton<ITimeProvider, SystemTimeProvider>();

        #region Fraud Rules Registration
        services.AddScoped<IFraudEvaluationRule, BlockedIpFraudRule>();
        services.AddScoped<IFraudEvaluationRule, RiskyHourFraudRule>();
        services.AddScoped<IFraudEvaluationRule, AccountAgeFraudRule>();
        services.AddScoped<IFraudEvaluationRule, KycLevelFraudRule>();
        #endregion

        return services;
    }
}

