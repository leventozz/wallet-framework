using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using System.Diagnostics;

namespace WF.WalletService.Api.Logging;

public static class LoggingDIExtensions
{
    public static IHostBuilder UseLogging(this IHostBuilder builder)
    {
        builder.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.WithThreadId()
                .Enrich.WithProperty("Application", context.HostingEnvironment.ApplicationName)
                .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                .Enrich.WithTraceId()
                .Enrich.WithSpanId();

            var serilogConfig = context.Configuration.GetSection("Serilog");
            if (serilogConfig.Exists())
            {
                configuration.ReadFrom.Configuration(context.Configuration, "Serilog");
            }
            else
            {
                // Default configuration if Serilog section doesn't exist
                configuration
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .MinimumLevel.Override("System", LogEventLevel.Warning)
                    .WriteTo.Console(
                        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{TraceId}] [{SpanId}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                    .WriteTo.File(
                        path: "logs/log-.txt",
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 7,
                        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] [{TraceId}] [{SpanId}] {Message:lj} {Properties:j}{NewLine}{Exception}");
            }
        });

        return builder;
    }

    public static LoggerConfiguration WithTraceId(this LoggerEnrichmentConfiguration enrichmentConfiguration)
    {
        return enrichmentConfiguration.With(new TraceIdEnricher());
    }

    public static LoggerConfiguration WithSpanId(this LoggerEnrichmentConfiguration enrichmentConfiguration)
    {
        return enrichmentConfiguration.With(new SpanIdEnricher());
    }
}

internal class TraceIdEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var activity = Activity.Current;
        if (activity != null)
        {
            var traceId = activity.TraceId.ToString();
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("TraceId", traceId));
        }
    }
}

internal class SpanIdEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var activity = Activity.Current;
        if (activity != null)
        {
            var spanId = activity.SpanId.ToString();
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("SpanId", spanId));
        }
    }
}

