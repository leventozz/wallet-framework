using WF.CustomerService.Api.Logging;
using WF.CustomerService.Api.Middleware;
using WF.CustomerService.Application;
using WF.CustomerService.Infrastructure;

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
