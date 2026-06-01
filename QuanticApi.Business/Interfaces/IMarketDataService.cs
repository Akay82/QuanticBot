using QuanticApi.Business.Contracts;

namespace QuanticApi.Business.Interfaces;

public interface IMarketDataService
{
    Task<MarketDataRefreshResponse> RefreshForexAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<ForexChartResponse>> GetForexChartsAsync(
        string timeframe,
        int candleCount,
        CancellationToken cancellationToken);

    Task<ForexChartResponse?> GetForexChartAsync(
        long instrumentId,
        string timeframe,
        int candleCount,
        CancellationToken cancellationToken);
}
