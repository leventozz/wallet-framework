using WF.CustomerService.Api.Extensions;
using WF.CustomerService.Api.Logging;
using WF.CustomerService.Api.Middleware;
using WF.CustomerService.Application;
using WF.CustomerService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddOpenTelemetry("CustomerService", "1.0.0");

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddWFExceptionHandler();
builder.Services.AddWFAuthentication(builder.Configuration, builder.Environment);


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
