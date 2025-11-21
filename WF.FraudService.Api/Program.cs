using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using WF.FraudService.Api.Logging;
using WF.FraudService.Application;
using WF.FraudService.Infrastructure;
using WF.FraudService.Middleware;
using WF.Shared.Observability;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseLogging();


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource =>
    {
        var attributes = OpenTelemetryConfig.GetResourceAttributes("FraudService", "1.0.0");
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
        
        tracing.AddSource("WF.FraudService");
        
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

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddWFExceptionHandler();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.UseWFExceptionHandler();

app.MapControllers();
app.MapPrometheusScrapingEndpoint();

app.Run();
