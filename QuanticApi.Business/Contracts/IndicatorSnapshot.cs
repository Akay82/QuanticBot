namespace QuanticApi.Business.Contracts;

public sealed record IndicatorCandle(
    DateTime CandleTime,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close);

public sealed record IndicatorSnapshot(
    DateTime CandleTime,
    decimal Close,
    decimal FastEma,
    decimal SlowEma,
    decimal Rsi,
    decimal Atr);

public sealed record RiskCalculation(
    decimal Quantity,
    decimal StopLoss,
    decimal TakeProfit);
