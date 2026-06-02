using QuanticApi.Business.Contracts;
using QuanticApi.Business.Exceptions;

namespace QuanticApi.Business.Services;

public static class IndicatorCalculator
{
    public static IndicatorSnapshot Calculate(
        IReadOnlyList<IndicatorCandle> candles,
        int fastEmaPeriod,
        int slowEmaPeriod,
        int rsiPeriod,
        int atrPeriod)
    {
        var required = new[] { fastEmaPeriod, slowEmaPeriod, rsiPeriod + 1, atrPeriod + 1 }.Max();
        if (candles.Count < required)
        {
            throw new BusinessRuleException($"At least {required} completed candles are required.");
        }

        var closes = candles.Select(item => item.Close).ToList();
        return new IndicatorSnapshot(
            candles[^1].CandleTime,
            closes[^1],
            CalculateEma(closes, fastEmaPeriod),
            CalculateEma(closes, slowEmaPeriod),
            CalculateRsi(closes, rsiPeriod),
            CalculateAtr(candles, atrPeriod));
    }

    private static decimal CalculateEma(IReadOnlyList<decimal> values, int period)
    {
        var multiplier = 2m / (period + 1);
        var ema = values.Take(period).Average();
        foreach (var value in values.Skip(period))
        {
            ema = (value - ema) * multiplier + ema;
        }

        return ema;
    }

    private static decimal CalculateRsi(IReadOnlyList<decimal> closes, int period)
    {
        var changes = closes.Zip(closes.Skip(1), (previous, current) => current - previous).TakeLast(period);
        var gains = changes.Where(change => change > 0).Sum();
        var losses = -changes.Where(change => change < 0).Sum();
        if (losses == 0)
        {
            return 100;
        }

        var relativeStrength = gains / losses;
        return 100 - 100 / (1 + relativeStrength);
    }

    private static decimal CalculateAtr(IReadOnlyList<IndicatorCandle> candles, int period)
    {
        var trueRanges = candles
            .Zip(candles.Skip(1), (previous, current) =>
                Math.Max(
                    current.High - current.Low,
                    Math.Max(Math.Abs(current.High - previous.Close), Math.Abs(current.Low - previous.Close))))
            .TakeLast(period);

        return trueRanges.Average();
    }
}
