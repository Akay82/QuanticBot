using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuanticApi.Business.Contracts;
using QuanticApi.Business.Exceptions;
using QuanticApi.Business.Interfaces;
using QuanticApi.Business.Models;
using QuanticApi.Business.Services;
using QuanticApi.Data.Persistence;

namespace QuanticApi.Data.Services;

public sealed class SwingBotService(
    TradingDbContext dbContext,
    IOrderWorkflowService orderWorkflowService,
    ILogger<SwingBotService> logger) : ISwingBotService
{
    private static readonly IReadOnlyDictionary<string, int> SupportedTimeframes =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["15m"] = 15,
            ["30m"] = 30,
            ["1h"] = 60,
            ["4h"] = 240,
            ["1d"] = 1440
        };

    public async Task<SwingBotSettingsResponse?> UpsertSettingsAsync(
        Guid botId,
        SwingBotSettingsRequest request,
        CancellationToken cancellationToken)
    {
        var bot = await dbContext.TradingBots.SingleOrDefaultAsync(item => item.Id == botId, cancellationToken);
        if (bot is null)
        {
            return null;
        }

        EnsureStopped(bot, "change settings");
        ValidateSettings(request);

        if (!await dbContext.PaperAccounts.AnyAsync(
                item => item.Id == request.AccountId && item.IsActive,
                cancellationToken))
        {
            throw new BusinessRuleException("An active paper account is required.");
        }

        if (!await dbContext.Instruments.AnyAsync(
                item => item.Id == request.InstrumentId && item.IsActive,
                cancellationToken))
        {
            throw new BusinessRuleException("An active instrument is required.");
        }

        var settings = await dbContext.BotStrategySettings.SingleOrDefaultAsync(
            item => item.BotId == botId,
            cancellationToken);

        if (settings is null)
        {
            settings = new BotStrategySettings { BotId = botId };
            dbContext.BotStrategySettings.Add(settings);
        }

        ApplySettings(settings, request);
        await AddLogAsync(botId, "INFO", "SETTINGS_UPDATED", "Swing strategy settings updated.", settings, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(settings);
    }

    public async Task<SwingBotDashboardResponse?> GetDashboardAsync(Guid botId, CancellationToken cancellationToken)
    {
        var bot = await dbContext.TradingBots.AsNoTracking().SingleOrDefaultAsync(item => item.Id == botId, cancellationToken);
        if (bot is null)
        {
            return null;
        }

        var settings = await dbContext.BotStrategySettings.AsNoTracking()
            .SingleOrDefaultAsync(item => item.BotId == botId, cancellationToken);
        var position = await dbContext.BotPositions.AsNoTracking()
            .SingleOrDefaultAsync(item => item.BotId == botId, cancellationToken);
        var logs = await GetLogsAsync(botId, 100, cancellationToken);

        return new SwingBotDashboardResponse(
            ToResponse(bot),
            settings is null ? null : ToResponse(settings),
            position is null ? null : ToResponse(position),
            logs);
    }

    public async Task<IReadOnlyList<BotLogResponse>> GetLogsAsync(
        Guid botId,
        int count,
        CancellationToken cancellationToken) =>
        await dbContext.BotLogs
            .AsNoTracking()
            .Where(item => item.BotId == botId)
            .OrderByDescending(item => item.CreatedAtUtc)
            .Take(Math.Clamp(count, 1, 500))
            .Select(item => ToResponse(item))
            .ToListAsync(cancellationToken);

    public async Task<TradingBotResponse?> StartAsync(Guid botId, CancellationToken cancellationToken)
    {
        var bot = await dbContext.TradingBots.SingleOrDefaultAsync(item => item.Id == botId, cancellationToken);
        if (bot is null)
        {
            return null;
        }

        if (!await dbContext.BotStrategySettings.AnyAsync(item => item.BotId == botId && item.IsEnabled, cancellationToken))
        {
            throw new BusinessRuleException("Configure enabled swing strategy settings before starting the bot.");
        }

        bot.Status = TradingBotStatus.Running;
        bot.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await AddLogAsync(botId, "INFO", "BOT_STARTED", "Swing bot started from the frontend.", null, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(bot);
    }

    public async Task<TradingBotResponse?> PauseAsync(Guid botId, CancellationToken cancellationToken)
    {
        var bot = await dbContext.TradingBots.SingleOrDefaultAsync(item => item.Id == botId, cancellationToken);
        if (bot is null)
        {
            return null;
        }

        bot.Status = TradingBotStatus.Stopped;
        bot.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await AddLogAsync(botId, "INFO", "BOT_PAUSED", "Swing bot paused from the frontend.", null, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(bot);
    }

    public async Task<BotEvaluationResponse?> EvaluateAsync(Guid botId, CancellationToken cancellationToken)
    {
        var bot = await dbContext.TradingBots.SingleOrDefaultAsync(item => item.Id == botId, cancellationToken);
        if (bot is null)
        {
            return null;
        }

        return await EvaluateBotAsync(bot, cancellationToken);
    }

    public async Task EvaluateRunningBotsAsync(CancellationToken cancellationToken)
    {
        var bots = await dbContext.TradingBots
            .Where(item => item.Status == TradingBotStatus.Running)
            .ToListAsync(cancellationToken);

        foreach (var bot in bots)
        {
            try
            {
                await EvaluateBotAsync(bot, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Swing bot {BotId} evaluation failed.", bot.Id);
                await AddLogAsync(bot.Id, "ERROR", "EVALUATION_FAILED", exception.Message, null, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }

    private async Task<BotEvaluationResponse> EvaluateBotAsync(TradingBot bot, CancellationToken cancellationToken)
    {
        if (bot.Status != TradingBotStatus.Running)
        {
            return new BotEvaluationResponse(bot.Id, "SKIPPED", "Bot is paused.");
        }

        var settings = await dbContext.BotStrategySettings.SingleOrDefaultAsync(
            item => item.BotId == bot.Id && item.IsEnabled,
            cancellationToken);

        if (settings is null)
        {
            return new BotEvaluationResponse(bot.Id, "SKIPPED", "Enabled settings were not found.");
        }

        var requiredCandles = new[]
        {
            settings.FastEmaPeriod,
            settings.SlowEmaPeriod,
            settings.RsiPeriod + 1,
            settings.AtrPeriod + 1
        }.Max();

        var candles = await GetCompletedCandlesAsync(
            settings.InstrumentId,
            settings.Timeframe,
            requiredCandles + 5,
            cancellationToken);

        if (candles.Count == 0)
        {
            return await LogOutcomeAsync(bot, "WAITING_FOR_DATA", "No completed candles are available yet.", null, cancellationToken);
        }

        var managedPosition = await dbContext.BotPositions.SingleOrDefaultAsync(
            item => item.BotId == bot.Id,
            cancellationToken);

        if (managedPosition is not null)
        {
            var protectionExit = await EvaluateProtectionExitAsync(bot, settings, managedPosition, cancellationToken);
            if (protectionExit is not null)
            {
                return protectionExit;
            }
        }

        if (bot.LastEvaluatedCandleUtc == candles[^1].CandleTime)
        {
            return new BotEvaluationResponse(bot.Id, "SKIPPED", "The latest completed candle was already evaluated.");
        }

        bot.LastEvaluatedCandleUtc = candles[^1].CandleTime;
        bot.UpdatedAtUtc = DateTimeOffset.UtcNow;

        if (candles.Count < requiredCandles)
        {
            return await LogOutcomeAsync(
                bot,
                "WAITING_FOR_HISTORY",
                $"Waiting for indicator history. Available={candles.Count}, Required={requiredCandles}.",
                null,
                cancellationToken);
        }

        var indicators = IndicatorCalculator.Calculate(
            candles,
            settings.FastEmaPeriod,
            settings.SlowEmaPeriod,
            settings.RsiPeriod,
            settings.AtrPeriod);

        return managedPosition is null
            ? await EvaluateEntryAsync(bot, settings, indicators, cancellationToken)
            : await EvaluateExitAsync(bot, settings, managedPosition, indicators, cancellationToken);
    }

    private async Task<BotEvaluationResponse> EvaluateEntryAsync(
        TradingBot bot,
        BotStrategySettings settings,
        IndicatorSnapshot indicators,
        CancellationToken cancellationToken)
    {
        var shouldBuy =
            indicators.FastEma > indicators.SlowEma &&
            indicators.Rsi >= settings.RsiEntryMin &&
            indicators.Rsi <= settings.RsiEntryMax &&
            indicators.Close > indicators.FastEma;

        if (!shouldBuy)
        {
            return await LogOutcomeAsync(bot, "NO_ENTRY", "Entry conditions were not met.", indicators, cancellationToken);
        }

        var account = await dbContext.PaperAccounts.SingleAsync(item => item.Id == settings.AccountId, cancellationToken);
        var capital = Math.Min(account.CurrentBalance, bot.Allocation);
        var risk = RiskManagerService.Calculate(
            capital,
            indicators.Close,
            indicators.Atr,
            settings.RiskPercent,
            settings.AtrStopMultiplier,
            settings.AtrTakeProfitMultiplier);

        if (risk.Quantity <= 0)
        {
            throw new BusinessRuleException("Calculated position quantity must be greater than zero.");
        }

        var order = new Order
        {
            AccountId = settings.AccountId,
            InstrumentId = settings.InstrumentId,
            OrderType = "MARKET",
            Side = "BUY",
            Quantity = risk.Quantity,
            RequestedPrice = indicators.Close
        };

        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);
        await orderWorkflowService.ExecuteAsync(order.Id, indicators.Close, cancellationToken);

        var position = await dbContext.Positions.SingleAsync(
            item => item.AccountId == settings.AccountId && item.InstrumentId == settings.InstrumentId,
            cancellationToken);

        dbContext.BotPositions.Add(new BotPosition
        {
            BotId = bot.Id,
            PositionId = position.Id,
            EntryPrice = indicators.Close,
            Quantity = risk.Quantity,
            StopLoss = risk.StopLoss,
            TakeProfit = risk.TakeProfit
        });

        return await LogOutcomeAsync(
            bot,
            "BUY_EXECUTED",
            $"BUY executed. Quantity={risk.Quantity}, Entry={indicators.Close}, StopLoss={risk.StopLoss}, TakeProfit={risk.TakeProfit}.",
            indicators,
            cancellationToken);
    }

    private async Task<BotEvaluationResponse> EvaluateExitAsync(
        TradingBot bot,
        BotStrategySettings settings,
        BotPosition managedPosition,
        IndicatorSnapshot indicators,
        CancellationToken cancellationToken)
    {
        var shouldSell =
            indicators.Close <= managedPosition.StopLoss ||
            indicators.Close >= managedPosition.TakeProfit ||
            indicators.Rsi >= settings.RsiExit ||
            indicators.FastEma < indicators.SlowEma;

        if (!shouldSell)
        {
            return await LogOutcomeAsync(bot, "HOLD", "Open position remains within strategy limits.", indicators, cancellationToken);
        }

        return await ExecuteSellAsync(bot, settings, managedPosition, indicators.Close, indicators, cancellationToken);
    }

    private async Task<BotEvaluationResponse?> EvaluateProtectionExitAsync(
        TradingBot bot,
        BotStrategySettings settings,
        BotPosition managedPosition,
        CancellationToken cancellationToken)
    {
        var latestPrice = await dbContext.PriceHistory
            .AsNoTracking()
            .Where(item => item.InstrumentId == settings.InstrumentId && item.Timeframe == "1m")
            .OrderByDescending(item => item.CandleTime)
            .Select(item => (decimal?)item.ClosePrice)
            .FirstOrDefaultAsync(cancellationToken);

        if (latestPrice is null ||
            latestPrice > managedPosition.StopLoss && latestPrice < managedPosition.TakeProfit)
        {
            return null;
        }

        return await ExecuteSellAsync(bot, settings, managedPosition, latestPrice.Value, null, cancellationToken);
    }

    private async Task<BotEvaluationResponse> ExecuteSellAsync(
        TradingBot bot,
        BotStrategySettings settings,
        BotPosition managedPosition,
        decimal exitPrice,
        object? details,
        CancellationToken cancellationToken)
    {
        var order = new Order
        {
            AccountId = settings.AccountId,
            InstrumentId = settings.InstrumentId,
            OrderType = "MARKET",
            Side = "SELL",
            Quantity = managedPosition.Quantity,
            RequestedPrice = exitPrice
        };

        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);
        await orderWorkflowService.ExecuteAsync(order.Id, exitPrice, cancellationToken);
        dbContext.BotPositions.Remove(managedPosition);

        return await LogOutcomeAsync(
            bot,
            "SELL_EXECUTED",
            $"SELL executed. Quantity={managedPosition.Quantity}, Exit={exitPrice}.",
            details,
            cancellationToken);
    }

    private async Task<IReadOnlyList<IndicatorCandle>> GetCompletedCandlesAsync(
        long instrumentId,
        string timeframe,
        int candleCount,
        CancellationToken cancellationToken)
    {
        var normalized = timeframe.Trim().ToLowerInvariant();
        if (!SupportedTimeframes.TryGetValue(normalized, out var bucketMinutes))
        {
            throw new BusinessRuleException($"Unsupported bot timeframe '{timeframe}'.");
        }

        var sourceCount = Math.Min(candleCount * bucketMinutes + bucketMinutes, 100000);
        var source = await dbContext.PriceHistory
            .AsNoTracking()
            .Where(item => item.InstrumentId == instrumentId && item.Timeframe == "1m")
            .OrderByDescending(item => item.CandleTime)
            .Take(sourceCount)
            .OrderBy(item => item.CandleTime)
            .ToListAsync(cancellationToken);

        var currentBucket = GetBucketStart(DateTime.UtcNow, bucketMinutes);
        return source
            .GroupBy(item => GetBucketStart(item.CandleTime, bucketMinutes))
            .Where(group => group.Key < currentBucket)
            .OrderBy(group => group.Key)
            .Select(group =>
            {
                var ordered = group.OrderBy(item => item.CandleTime).ToList();
                return new IndicatorCandle(
                    group.Key,
                    ordered[0].OpenPrice,
                    ordered.Max(item => item.HighPrice),
                    ordered.Min(item => item.LowPrice),
                    ordered[^1].ClosePrice);
            })
            .TakeLast(candleCount)
            .ToList();
    }

    private async Task<BotEvaluationResponse> LogOutcomeAsync(
        TradingBot bot,
        string outcome,
        string message,
        object? details,
        CancellationToken cancellationToken)
    {
        await AddLogAsync(bot.Id, "INFO", outcome, message, details, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new BotEvaluationResponse(bot.Id, outcome, message);
    }

    private async Task AddLogAsync(
        Guid botId,
        string level,
        string eventType,
        string message,
        object? details,
        CancellationToken cancellationToken)
    {
        await dbContext.BotLogs.AddAsync(new BotLog
        {
            BotId = botId,
            Level = level,
            EventType = eventType,
            Message = message,
            Details = details is null ? null : JsonSerializer.Serialize(details)
        }, cancellationToken);
    }

    private static void ApplySettings(BotStrategySettings settings, SwingBotSettingsRequest request)
    {
        settings.AccountId = request.AccountId;
        settings.InstrumentId = request.InstrumentId;
        settings.Timeframe = request.Timeframe.Trim().ToLowerInvariant();
        settings.FastEmaPeriod = request.FastEmaPeriod;
        settings.SlowEmaPeriod = request.SlowEmaPeriod;
        settings.RsiPeriod = request.RsiPeriod;
        settings.AtrPeriod = request.AtrPeriod;
        settings.RsiEntryMin = request.RsiEntryMin;
        settings.RsiEntryMax = request.RsiEntryMax;
        settings.RsiExit = request.RsiExit;
        settings.AtrStopMultiplier = request.AtrStopMultiplier;
        settings.AtrTakeProfitMultiplier = request.AtrTakeProfitMultiplier;
        settings.RiskPercent = request.RiskPercent;
        settings.IsEnabled = true;
        settings.UpdatedAtUtc = DateTime.UtcNow;
    }

    private static void ValidateSettings(SwingBotSettingsRequest request)
    {
        if (!SupportedTimeframes.ContainsKey(request.Timeframe.Trim()))
        {
            throw new BusinessRuleException($"Use one of these bot timeframes: {string.Join(", ", SupportedTimeframes.Keys)}.");
        }

        if (request.FastEmaPeriod <= 0 ||
            request.SlowEmaPeriod <= request.FastEmaPeriod ||
            request.RsiPeriod <= 0 ||
            request.AtrPeriod <= 0 ||
            request.RiskPercent is <= 0 or > 100 ||
            request.AtrStopMultiplier <= 0 ||
            request.AtrTakeProfitMultiplier <= 0)
        {
            throw new BusinessRuleException("Swing strategy settings are invalid.");
        }
    }

    private static void EnsureStopped(TradingBot bot, string operation)
    {
        if (bot.Status == TradingBotStatus.Running)
        {
            throw new BusinessRuleException($"Pause the bot before attempting to {operation}.");
        }
    }

    private static DateTime GetBucketStart(DateTime candleTime, int bucketMinutes)
    {
        var utc = candleTime.Kind == DateTimeKind.Utc
            ? candleTime
            : DateTime.SpecifyKind(candleTime, DateTimeKind.Utc);
        var ticks = TimeSpan.FromMinutes(bucketMinutes).Ticks;
        return new DateTime(utc.Ticks - utc.Ticks % ticks, DateTimeKind.Utc);
    }

    private static TradingBotResponse ToResponse(TradingBot bot) =>
        new(
            bot.Id,
            bot.Name,
            bot.Symbol,
            bot.Exchange,
            bot.Strategy,
            bot.Allocation,
            bot.Status,
            bot.LastEvaluatedCandleUtc,
            bot.CreatedAtUtc,
            bot.UpdatedAtUtc);

    private static SwingBotSettingsResponse ToResponse(BotStrategySettings settings) =>
        new(
            settings.BotId,
            settings.AccountId,
            settings.InstrumentId,
            settings.Timeframe,
            settings.FastEmaPeriod,
            settings.SlowEmaPeriod,
            settings.RsiPeriod,
            settings.AtrPeriod,
            settings.RsiEntryMin,
            settings.RsiEntryMax,
            settings.RsiExit,
            settings.AtrStopMultiplier,
            settings.AtrTakeProfitMultiplier,
            settings.RiskPercent,
            settings.IsEnabled,
            settings.UpdatedAtUtc);

    private static BotPositionResponse ToResponse(BotPosition position) =>
        new(
            position.PositionId,
            position.EntryPrice,
            position.Quantity,
            position.StopLoss,
            position.TakeProfit,
            position.OpenedAtUtc,
            position.UpdatedAtUtc);

    private static BotLogResponse ToResponse(BotLog log) =>
        new(log.Id, log.Level, log.EventType, log.Message, log.Details, log.CreatedAtUtc);
}
