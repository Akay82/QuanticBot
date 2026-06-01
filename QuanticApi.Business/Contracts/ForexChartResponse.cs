namespace QuanticApi.Business.Contracts;

public sealed record ForexCandleResponse(
    DateTime CandleTime,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    decimal? Volume);

public sealed record ForexChartResponse(
    long InstrumentId,
    string Symbol,
    string? Name,
    string Timeframe,
    IReadOnlyList<ForexCandleResponse> Candles);

public sealed record MarketDataRefreshResponse(
    bool IsEnabled,
    int RequestedPairs,
    int UpdatedPairs,
    int SavedCandles,
    IReadOnlyList<string> Errors);
