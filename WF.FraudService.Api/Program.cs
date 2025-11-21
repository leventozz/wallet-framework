using WF.FraudService.Api.Extensions;
using WF.FraudService.Api.Logging;
using WF.FraudService.Application;
using WF.FraudService.Infrastructure;
using WF.FraudService.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseLogging();


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddWFApiVersioning();
builder.Services.AddOpenTelemetry("FraudService", "1.0.0");

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
