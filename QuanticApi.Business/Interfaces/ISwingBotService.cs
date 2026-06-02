using QuanticApi.Business.Contracts;

namespace QuanticApi.Business.Interfaces;

public interface ISwingBotService
{
    Task<SwingBotSettingsResponse?> UpsertSettingsAsync(
        Guid botId,
        SwingBotSettingsRequest request,
        CancellationToken cancellationToken);

    Task<SwingBotDashboardResponse?> GetDashboardAsync(Guid botId, CancellationToken cancellationToken);
    Task<IReadOnlyList<BotLogResponse>> GetLogsAsync(Guid botId, int count, CancellationToken cancellationToken);
    Task<TradingBotResponse?> StartAsync(Guid botId, CancellationToken cancellationToken);
    Task<TradingBotResponse?> PauseAsync(Guid botId, CancellationToken cancellationToken);
    Task<BotEvaluationResponse?> EvaluateAsync(Guid botId, CancellationToken cancellationToken);
    Task EvaluateRunningBotsAsync(CancellationToken cancellationToken);
}
