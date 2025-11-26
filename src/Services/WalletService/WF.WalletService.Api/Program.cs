using WF.Shared.Contracts.Abstractions;
using WF.WalletService.Api.Extensions;
using WF.WalletService.Api.Logging;
using WF.WalletService.Api.Services;
using WF.WalletService.Application;
using WF.WalletService.Infrastructure;
using WF.WalletService.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddVaultConfiguration(builder.Configuration);

builder.Host.UseLogging();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ServiceToService", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("azp",
            "wallet-client"
            //add more clients
        );
    });
});

builder.Services.AddWFApiVersioning();
builder.Services.AddOpenTelemetry("WalletService", "1.0.0");

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddWFExceptionHandler();
builder.Services.AddWFAuthentication(builder.Configuration, builder.Environment);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapPrometheusScrapingEndpoint();

app.UseWFExceptionHandler();

app.Run();
