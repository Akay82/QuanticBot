using QuanticApi.Business.Interfaces;

namespace QuanticApi.HostedServices;

public sealed class SwingBotWorker(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<SwingBotWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalSeconds = Math.Max(30, configuration.GetValue("SwingBots:EvaluationIntervalSeconds", 60));
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(intervalSeconds));

        do
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<ISwingBotService>();
                await service.EvaluateRunningBotsAsync(stoppingToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Swing bot worker cycle failed.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
