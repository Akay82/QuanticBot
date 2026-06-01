using Microsoft.AspNetCore.Mvc;
using QuanticApi.Business.Contracts;
using QuanticApi.Business.Interfaces;
using QuanticApi.Business.Models;

namespace QuanticApi.Controllers;

[Route("api/instruments")]
public sealed class InstrumentsController(ICrudService<Instrument> service) : CrudControllerBase<Instrument>(service)
{
    [HttpGet]
    public Task<ActionResult<PagedResponse<Instrument>>> GetAll(
        [FromQuery] PageRequest page,
        string? symbol,
        string? marketType,
        string? exchange,
        bool? isActive,
        CancellationToken cancellationToken) =>
        GetPage(page, item =>
            (symbol == null || item.Symbol.Contains(symbol)) &&
            (marketType == null || item.MarketType == marketType) &&
            (exchange == null || item.Exchange == exchange) &&
            (isActive == null || item.IsActive == isActive), cancellationToken);
}

[Route("api/paper-accounts")]
public sealed class PaperAccountsController(ICrudService<PaperAccount> service) : CrudControllerBase<PaperAccount>(service)
{
    [HttpGet]
    public Task<ActionResult<PagedResponse<PaperAccount>>> GetAll(
        [FromQuery] PageRequest page,
        long? userId,
        string? currency,
        bool? isActive,
        CancellationToken cancellationToken) =>
        GetPage(page, item =>
            (userId == null || item.UserId == userId) &&
            (currency == null || item.Currency == currency) &&
            (isActive == null || item.IsActive == isActive), cancellationToken);
}

[Route("api/strategies")]
public sealed class StrategiesController(ICrudService<Strategy> service) : CrudControllerBase<Strategy>(service)
{
    [HttpGet]
    public Task<ActionResult<PagedResponse<Strategy>>> GetAll(
        [FromQuery] PageRequest page,
        long? userId,
        string? strategyType,
        bool? isActive,
        CancellationToken cancellationToken) =>
        GetPage(page, item =>
            (userId == null || item.UserId == userId) &&
            (strategyType == null || item.StrategyType == strategyType) &&
            (isActive == null || item.IsActive == isActive), cancellationToken);
}

[Route("api/orders")]
public sealed class OrdersController(
    ICrudService<Order> service,
    IOrderWorkflowService workflowService) : CrudControllerBase<Order>(service)
{
    [HttpGet]
    public Task<ActionResult<PagedResponse<Order>>> GetAll(
        [FromQuery] PageRequest page,
        long? accountId,
        long? strategyId,
        long? instrumentId,
        string? status,
        string? side,
        string? orderType,
        CancellationToken cancellationToken) =>
        GetPage(page, item =>
            (accountId == null || item.AccountId == accountId) &&
            (strategyId == null || item.StrategyId == strategyId) &&
            (instrumentId == null || item.InstrumentId == instrumentId) &&
            (status == null || item.Status == status) &&
            (side == null || item.Side == side) &&
            (orderType == null || item.OrderType == orderType), cancellationToken);

    [HttpPost("{id:long}/cancel")]
    public async Task<IActionResult> Cancel(long id, CancellationToken cancellationToken)
    {
        await workflowService.CancelAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:long}/execute")]
    public async Task<IActionResult> Execute(
        long id,
        ExecuteOrderRequest request,
        CancellationToken cancellationToken)
    {
        await workflowService.ExecuteAsync(id, request.ExecutedPrice, cancellationToken);
        return NoContent();
    }
}

[Route("api/positions")]
public sealed class PositionsController(ICrudService<Position> service) : CrudControllerBase<Position>(service)
{
    [HttpGet]
    public Task<ActionResult<PagedResponse<Position>>> GetAll(
        [FromQuery] PageRequest page,
        long? accountId,
        long? instrumentId,
        CancellationToken cancellationToken) =>
        GetPage(page, item =>
            (accountId == null || item.AccountId == accountId) &&
            (instrumentId == null || item.InstrumentId == instrumentId), cancellationToken);
}

[Route("api/price-history")]
public sealed class PriceHistoryController(ICrudService<PriceHistory> service) : CrudControllerBase<PriceHistory>(service)
{
    [HttpGet]
    public Task<ActionResult<PagedResponse<PriceHistory>>> GetAll(
        [FromQuery] PageRequest page,
        long? instrumentId,
        string? timeframe,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken) =>
        GetPage(page, item =>
            (instrumentId == null || item.InstrumentId == instrumentId) &&
            (timeframe == null || item.Timeframe == timeframe) &&
            (from == null || item.CandleTime >= from) &&
            (to == null || item.CandleTime <= to), cancellationToken);
}

[Route("api/signals")]
public sealed class SignalsController(ICrudService<Signal> service) : CrudControllerBase<Signal>(service)
{
    [HttpGet]
    public Task<ActionResult<PagedResponse<Signal>>> GetAll(
        [FromQuery] PageRequest page,
        long? strategyId,
        long? instrumentId,
        string? signalType,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken) =>
        GetPage(page, item =>
            (strategyId == null || item.StrategyId == strategyId) &&
            (instrumentId == null || item.InstrumentId == instrumentId) &&
            (signalType == null || item.SignalType == signalType) &&
            (from == null || item.CreatedAt >= from) &&
            (to == null || item.CreatedAt <= to), cancellationToken);
}

[Route("api/trades")]
public sealed class TradesController(ICrudService<Trade> service) : CrudControllerBase<Trade>(service)
{
    [HttpGet]
    public Task<ActionResult<PagedResponse<Trade>>> GetAll(
        [FromQuery] PageRequest page,
        long? orderId,
        long? accountId,
        long? instrumentId,
        string? side,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken) =>
        GetPage(page, item =>
            (orderId == null || item.OrderId == orderId) &&
            (accountId == null || item.AccountId == accountId) &&
            (instrumentId == null || item.InstrumentId == instrumentId) &&
            (side == null || item.Side == side) &&
            (from == null || item.TradeTime >= from) &&
            (to == null || item.TradeTime <= to), cancellationToken);
}

[Route("api/watchlists")]
public sealed class WatchlistsController(ICrudService<Watchlist> service) : CrudControllerBase<Watchlist>(service)
{
    [HttpGet]
    public Task<ActionResult<PagedResponse<Watchlist>>> GetAll(
        [FromQuery] PageRequest page,
        long? userId,
        long? instrumentId,
        CancellationToken cancellationToken) =>
        GetPage(page, item =>
            (userId == null || item.UserId == userId) &&
            (instrumentId == null || item.InstrumentId == instrumentId), cancellationToken);
}

public sealed record ExecuteOrderRequest(decimal ExecutedPrice);
