using Microsoft.AspNetCore.Mvc;
using QuanticApi.Business.Contracts;
using QuanticApi.Business.Interfaces;

namespace QuanticApi.Controllers;

[ApiController]
[Route("api/market-data/forex")]
public sealed class MarketDataController(IMarketDataService service) : ControllerBase
{
    [HttpGet("charts")]
    public async Task<ActionResult<IReadOnlyList<ForexChartResponse>>> GetCharts(
        string timeframe = "1m",
        int candleCount = 120,
        CancellationToken cancellationToken = default) =>
        Ok(await service.GetForexChartsAsync(timeframe, candleCount, cancellationToken));

    [HttpGet("charts/{instrumentId:long}")]
    public async Task<ActionResult<ForexChartResponse>> GetChart(
        long instrumentId,
        string timeframe = "1m",
        int candleCount = 120,
        CancellationToken cancellationToken = default)
    {
        var chart = await service.GetForexChartAsync(instrumentId, timeframe, candleCount, cancellationToken);
        return chart is null ? NotFound() : Ok(chart);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<MarketDataRefreshResponse>> Refresh(CancellationToken cancellationToken) =>
        Ok(await service.RefreshForexAsync(cancellationToken));
}
