using WF.WalletService.Api.Extensions;
using WF.WalletService.Api.Logging;
using WF.WalletService.Application;
using WF.WalletService.Infrastructure;
using WF.WalletService.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseLogging();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddWFApiVersioning();
builder.Services.AddOpenTelemetry("WalletService", "1.0.0");

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

app.MapControllers();
app.MapPrometheusScrapingEndpoint();

app.UseWFExceptionHandler();

app.Run();
