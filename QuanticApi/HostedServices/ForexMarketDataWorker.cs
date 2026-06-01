using Microsoft.Extensions.Options;
using QuanticApi.Business.Interfaces;
using QuanticApi.Data.MarketData;

namespace QuanticApi.HostedServices;

public sealed class ForexMarketDataWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<TwelveDataOptions> options,
    ILogger<ForexMarketDataWorker> logger) : BackgroundService
{
    private readonly TwelveDataOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.EnableWorker)
        {
            logger.LogInformation("Twelve Data forex worker is disabled by configuration.");
            return;
        }

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            logger.LogWarning("Twelve Data forex worker is waiting for configuration. Set TwelveData__ApiKey.");
            return;
        }

        var interval = TimeSpan.FromSeconds(Math.Max(60, _options.RefreshIntervalSeconds));
        using var timer = new PeriodicTimer(interval);

        do
        {
            await RefreshAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task RefreshAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IMarketDataService>();
        var result = await service.RefreshForexAsync(cancellationToken);

        logger.LogInformation(
            "Twelve Data forex refresh completed. UpdatedPairs={UpdatedPairs}, SavedCandles={SavedCandles}, Errors={ErrorCount}",
            result.UpdatedPairs,
            result.SavedCandles,
            result.Errors.Count);
    }
}
