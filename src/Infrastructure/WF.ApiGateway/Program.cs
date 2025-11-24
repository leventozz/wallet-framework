using AspNetCoreRateLimit;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using WF.ApiGateway.Extensions;
using WF.Shared.Observability;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource =>
    {
        var attributes = OpenTelemetryConfig.GetResourceAttributes("ApiGateway", "1.0.0");
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
        
        tracing.AddSource("WF.ApiGateway");
        
        tracing.AddAspNetCoreInstrumentation();
        tracing.AddHttpClientInstrumentation();
        
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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));


builder.Services.AddCors(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    }
});

builder.Services.AddRedis(builder.Configuration);
builder.Services.AddRateLimiting(builder.Configuration);

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

if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowAll");
}

app.UseIpRateLimiting();

app.UseAuthorization();

app.MapControllers();
app.MapPrometheusScrapingEndpoint();
app.MapReverseProxy();

app.Run();
