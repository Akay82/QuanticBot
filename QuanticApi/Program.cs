using Microsoft.EntityFrameworkCore;
using QuanticApi.Business.Interfaces;
using QuanticApi.Business.Services;
using QuanticApi.Data.Persistence;
using QuanticApi.Data.Repositories;
using QuanticApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

var connectionString = builder.Configuration.GetConnectionString("TradingDatabase")
    ?? throw new InvalidOperationException("Connection string 'TradingDatabase' is not configured.");

builder.Services.AddDbContext<TradingDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<ITradingBotRepository, TradingBotRepository>();
builder.Services.AddScoped<ITradingBotService, TradingBotService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
