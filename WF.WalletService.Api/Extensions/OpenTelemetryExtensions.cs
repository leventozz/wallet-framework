using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using WF.Shared.Observability;

namespace WF.WalletService.Api.Extensions;

public static class OpenTelemetryExtensions
{
    public static IServiceCollection AddOpenTelemetry(this IServiceCollection services, string serviceName, string version)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                var attributes = OpenTelemetryConfig.GetResourceAttributes(serviceName, version);
                resource.AddAttributes(
                    attributes.Select(kv => new KeyValuePair<string, object>(kv.Key, kv.Value))
                );
            })
            .WithTracing(tracing =>
            {
                foreach (var source in OpenTelemetryConfig.CommonActivitySources)
                {
                    tracing.AddSource(source);
                }
                
                tracing.AddSource($"WF.{serviceName}");
                
                tracing.AddAspNetCoreInstrumentation();
                tracing.AddHttpClientInstrumentation();
                tracing.AddEntityFrameworkCoreInstrumentation();
                
                tracing.AddOtlpExporter(opts =>
                {
                    opts.Endpoint = new Uri(OpenTelemetryConfig.OtlpEndpoint);
                });
            })
            .WithMetrics(metrics => 
            {
                metrics.AddAspNetCoreInstrumentation();
                metrics.AddHttpClientInstrumentation();
                metrics.AddRuntimeInstrumentation();
                metrics.AddPrometheusExporter();  
            });

        return services;
    }
}

