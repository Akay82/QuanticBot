using QuanticApi.Business.Models;

namespace QuanticApi.Business.Contracts;

public sealed record SwingBotSettingsRequest(
    long AccountId,
    long InstrumentId,
    string Timeframe = "4h",
    int FastEmaPeriod = 50,
    int SlowEmaPeriod = 200,
    int RsiPeriod = 14,
    int AtrPeriod = 14,
    decimal RsiEntryMin = 40,
    decimal RsiEntryMax = 55,
    decimal RsiExit = 70,
    decimal AtrStopMultiplier = 1.5m,
    decimal AtrTakeProfitMultiplier = 3,
    decimal RiskPercent = 1);

public sealed record SwingBotSettingsResponse(
    Guid BotId,
    long AccountId,
    long InstrumentId,
    string Timeframe,
    int FastEmaPeriod,
    int SlowEmaPeriod,
    int RsiPeriod,
    int AtrPeriod,
    decimal RsiEntryMin,
    decimal RsiEntryMax,
    decimal RsiExit,
    decimal AtrStopMultiplier,
    decimal AtrTakeProfitMultiplier,
    decimal RiskPercent,
    bool IsEnabled,
    DateTime UpdatedAtUtc);

public sealed record BotPositionResponse(
    long PositionId,
    decimal EntryPrice,
    decimal Quantity,
    decimal StopLoss,
    decimal TakeProfit,
    DateTime OpenedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record BotLogResponse(
    long Id,
    string Level,
    string EventType,
    string Message,
    string? Details,
    DateTime CreatedAtUtc);

public sealed record SwingBotDashboardResponse(
    TradingBotResponse Bot,
    SwingBotSettingsResponse? Settings,
    BotPositionResponse? Position,
    IReadOnlyList<BotLogResponse> RecentLogs);

public sealed record BotEvaluationResponse(Guid BotId, string Outcome, string Message);
