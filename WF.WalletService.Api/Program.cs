using WF.Shared.Infrastructure.Logging;
using WF.WalletService.Application;
using WF.WalletService.Infrastructure;
using WF.WalletService.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseLogging();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddWFExceptionHandler();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseWFExceptionHandler();

app.Run();
