using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuanticApi.Business.Contracts;
using QuanticApi.Business.Interfaces;
using QuanticApi.Business.Models;
using QuanticApi.Data.MarketData;
using QuanticApi.Data.Persistence;

namespace QuanticApi.Data.Services;

public sealed class MarketDataService(
    TradingDbContext dbContext,
    TwelveDataForexClient twelveDataClient,
    IOptions<TwelveDataOptions> options,
    ILogger<MarketDataService> logger) : IMarketDataService
{
    private static readonly IReadOnlyDictionary<string, int> SupportedTimeframes =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["1m"] = 1,
            ["5m"] = 5,
            ["15m"] = 15,
            ["30m"] = 30,
            ["1h"] = 60,
            ["4h"] = 240,
            ["1d"] = 1440
        };

    private readonly TwelveDataOptions _options = options.Value;

    public async Task<MarketDataRefreshResponse> RefreshForexAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return new MarketDataRefreshResponse(false, _options.ForexPairs.Count, 0, 0, []);
        }

        var errors = new List<string>();
        var updatedPairs = 0;
        var savedCandles = 0;

        foreach (var pair in _options.ForexPairs)
        {
            try
            {
                var instrument = await GetOrCreateInstrumentAsync(pair, cancellationToken);
                var hasCandles = await dbContext.PriceHistory.AnyAsync(
                    item => item.InstrumentId == instrument.Id && item.Timeframe == _options.StoredTimeframe,
                    cancellationToken);

                var response = await twelveDataClient.GetTimeSeriesAsync(
                    pair.ProviderSymbol,
                    _options.Interval,
                    hasCandles ? _options.IncrementalOutputSize : _options.InitialOutputSize,
                    cancellationToken);

                savedCandles += await SaveCandlesAsync(instrument.Id, response.Values, cancellationToken);
                updatedPairs++;
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Twelve Data refresh failed for {Symbol}.", pair.Symbol);
                errors.Add($"{pair.Symbol}: {exception.Message}");
            }
        }

        return new MarketDataRefreshResponse(true, _options.ForexPairs.Count, updatedPairs, savedCandles, errors);
    }

    public async Task<IReadOnlyList<ForexChartResponse>> GetForexChartsAsync(
        string timeframe,
        int candleCount,
        CancellationToken cancellationToken)
    {
        var configuredSymbols = _options.ForexPairs.Select(item => item.Symbol).ToList();
        var instruments = await dbContext.Instruments
            .AsNoTracking()
            .Where(item =>
                item.MarketType == "FOREX" &&
                item.Exchange == TwelveDataOptions.ExchangeName &&
                item.IsActive &&
                configuredSymbols.Contains(item.Symbol))
            .OrderBy(item => item.Id)
            .ToListAsync(cancellationToken);

        var charts = new List<ForexChartResponse>();
        foreach (var instrument in instruments)
        {
            charts.Add(await BuildChartAsync(instrument, timeframe, candleCount, cancellationToken));
        }

        return charts;
    }

    public async Task<ForexChartResponse?> GetForexChartAsync(
        long instrumentId,
        string timeframe,
        int candleCount,
        CancellationToken cancellationToken)
    {
        var instrument = await dbContext.Instruments
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item =>
                    item.Id == instrumentId &&
                    item.MarketType == "FOREX" &&
                    item.Exchange == TwelveDataOptions.ExchangeName &&
                    item.IsActive,
                cancellationToken);

        return instrument is null
            ? null
            : await BuildChartAsync(instrument, timeframe, candleCount, cancellationToken);
    }

    private async Task<Instrument> GetOrCreateInstrumentAsync(
        ForexPairOptions pair,
        CancellationToken cancellationToken)
    {
        var instrument = await dbContext.Instruments.SingleOrDefaultAsync(
            item => item.Symbol == pair.Symbol && item.Exchange == TwelveDataOptions.ExchangeName,
            cancellationToken);

        if (instrument is not null)
        {
            return instrument;
        }

        instrument = new Instrument
        {
            Symbol = pair.Symbol,
            Name = pair.Name,
            MarketType = "FOREX",
            Exchange = TwelveDataOptions.ExchangeName,
            BaseCurrency = pair.BaseCurrency,
            QuoteCurrency = pair.QuoteCurrency
        };

        dbContext.Instruments.Add(instrument);
        await dbContext.SaveChangesAsync(cancellationToken);
        return instrument;
    }

    private async Task<int> SaveCandlesAsync(
        long instrumentId,
        IReadOnlyList<TwelveDataCandle> values,
        CancellationToken cancellationToken)
    {
        var candles = values
            .Select(ParseCandle)
            .Where(item => item is not null)
            .Cast<PriceHistory>()
            .ToList();

        if (candles.Count == 0)
        {
            return 0;
        }

        var candleTimes = candles.Select(item => item.CandleTime).ToList();
        var existingTimes = await dbContext.PriceHistory
            .Where(item =>
                item.InstrumentId == instrumentId &&
                item.Timeframe == _options.StoredTimeframe &&
                candleTimes.Contains(item.CandleTime))
            .Select(item => item.CandleTime)
            .ToListAsync(cancellationToken);

        var existing = existingTimes.ToHashSet();
        foreach (var candle in candles)
        {
            if (existing.Contains(candle.CandleTime))
            {
                continue;
            }

            candle.InstrumentId = instrumentId;
            candle.Timeframe = _options.StoredTimeframe;
            dbContext.PriceHistory.Add(candle);
        }

        return await dbContext.SaveChangesAsync(cancellationToken);
    }

    private PriceHistory? ParseCandle(TwelveDataCandle value)
    {
        const NumberStyles numberStyles = NumberStyles.Number;
        var dateStyles = DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal;

        if (!DateTime.TryParse(value.Datetime, CultureInfo.InvariantCulture, dateStyles, out var candleTime) ||
            !decimal.TryParse(value.Open, numberStyles, CultureInfo.InvariantCulture, out var open) ||
            !decimal.TryParse(value.High, numberStyles, CultureInfo.InvariantCulture, out var high) ||
            !decimal.TryParse(value.Low, numberStyles, CultureInfo.InvariantCulture, out var low) ||
            !decimal.TryParse(value.Close, numberStyles, CultureInfo.InvariantCulture, out var close))
        {
            logger.LogWarning("Skipping malformed Twelve Data candle at {Datetime}.", value.Datetime);
            return null;
        }

        decimal? volume = decimal.TryParse(value.Volume, numberStyles, CultureInfo.InvariantCulture, out var parsedVolume)
            ? parsedVolume
            : null;

        return new PriceHistory
        {
            OpenPrice = open,
            HighPrice = high,
            LowPrice = low,
            ClosePrice = close,
            Volume = volume,
            CandleTime = candleTime
        };
    }

    private async Task<ForexChartResponse> BuildChartAsync(
        Instrument instrument,
        string timeframe,
        int candleCount,
        CancellationToken cancellationToken)
    {
        var normalizedTimeframe = NormalizeTimeframe(timeframe);
        var bucketSizeMinutes = SupportedTimeframes[normalizedTimeframe];
        var safeCandleCount = Math.Clamp(candleCount, 1, 1000);
        var sourceCandleCount = Math.Min(safeCandleCount * bucketSizeMinutes + bucketSizeMinutes, 100000);
        var sourceCandles = await dbContext.PriceHistory
            .AsNoTracking()
            .Where(item => item.InstrumentId == instrument.Id && item.Timeframe == _options.StoredTimeframe)
            .OrderByDescending(item => item.CandleTime)
            .Take(sourceCandleCount)
            .OrderBy(item => item.CandleTime)
            .ToListAsync(cancellationToken);

        var candles = sourceCandles
            .GroupBy(item => GetBucketStart(item.CandleTime, bucketSizeMinutes))
            .OrderBy(group => group.Key)
            .Select(group =>
            {
                var ordered = group.OrderBy(item => item.CandleTime).ToList();
                return new ForexCandleResponse(
                    group.Key,
                    ordered[0].OpenPrice,
                    ordered.Max(item => item.HighPrice),
                    ordered.Min(item => item.LowPrice),
                    ordered[^1].ClosePrice,
                    ordered.All(item => item.Volume is null) ? null : ordered.Sum(item => item.Volume ?? 0));
            })
            .TakeLast(safeCandleCount)
            .ToList();

        return new ForexChartResponse(
            instrument.Id,
            instrument.Symbol,
            instrument.Name,
            normalizedTimeframe,
            candles);
    }

    private static string NormalizeTimeframe(string timeframe)
    {
        var normalized = timeframe.Trim().ToLowerInvariant();
        if (!SupportedTimeframes.ContainsKey(normalized))
        {
            throw new ArgumentException(
                $"Unsupported timeframe '{timeframe}'. Use one of: {string.Join(", ", SupportedTimeframes.Keys)}.");
        }

        return normalized;
    }

    private static DateTime GetBucketStart(DateTime candleTime, int bucketSizeMinutes)
    {
        var utc = candleTime.Kind == DateTimeKind.Utc
            ? candleTime
            : DateTime.SpecifyKind(candleTime, DateTimeKind.Utc);

        var ticksPerBucket = TimeSpan.FromMinutes(bucketSizeMinutes).Ticks;
        return new DateTime(utc.Ticks - utc.Ticks % ticksPerBucket, DateTimeKind.Utc);
    }
}
