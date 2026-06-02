using Microsoft.EntityFrameworkCore;
using QuanticApi.Business.Interfaces;
using QuanticApi.Business.Services;
using QuanticApi.Data.Persistence;
using QuanticApi.Data.Repositories;
using QuanticApi.Data.Services;
using QuanticApi.Data.MarketData;
using QuanticApi.HostedServices;
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
builder.Services.AddScoped(typeof(ICrudRepository<>), typeof(CrudRepository<>));
builder.Services.AddScoped(typeof(ICrudService<>), typeof(CrudService<>));
builder.Services.AddScoped<IOrderWorkflowService, OrderWorkflowService>();
builder.Services.Configure<TwelveDataOptions>(builder.Configuration.GetSection(TwelveDataOptions.SectionName));
builder.Services.AddHttpClient<TwelveDataForexClient>(client =>
{
    client.BaseAddress = new Uri("https://api.twelvedata.com/");
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddScoped<IMarketDataService, MarketDataService>();
builder.Services.AddScoped<ISwingBotService, SwingBotService>();
builder.Services.AddHostedService<ForexMarketDataWorker>();
builder.Services.AddHostedService<SwingBotWorker>();

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
