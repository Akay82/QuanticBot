using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using QuanticApi.Business.Contracts;
using QuanticApi.Business.Interfaces;

namespace QuanticApi.Controllers;

[ApiController]
[Route("api/trading-bots")]
public sealed class TradingBotsController(
    ITradingBotService service,
    ISwingBotService swingBotService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TradingBotResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await service.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TradingBotResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var bot = await service.GetByIdAsync(id, cancellationToken);
        return bot is null ? NotFound() : Ok(bot);
    }

    [HttpPost]
    public async Task<ActionResult<TradingBotResponse>> Create(
        CreateTradingBotRequest request,
        CancellationToken cancellationToken)
    {
        var bot = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = bot.Id }, bot);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TradingBotResponse>> Update(
        Guid id,
        UpdateTradingBotRequest request,
        CancellationToken cancellationToken)
    {
        var bot = await service.UpdateAsync(id, request, cancellationToken);
        return bot is null ? NotFound() : Ok(bot);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        await service.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();

    [HttpPost("{id:guid}/start")]
    public async Task<ActionResult<TradingBotResponse>> Start(Guid id, CancellationToken cancellationToken)
    {
        var bot = await swingBotService.StartAsync(id, cancellationToken);
        return bot is null ? NotFound() : Ok(bot);
    }

    [HttpPost("{id:guid}/pause")]
    [HttpPost("{id:guid}/stop")]
    public async Task<ActionResult<TradingBotResponse>> Pause(Guid id, CancellationToken cancellationToken)
    {
        var bot = await swingBotService.PauseAsync(id, cancellationToken);
        return bot is null ? NotFound() : Ok(bot);
    }

    [HttpPut("{id:guid}/settings")]
    public async Task<ActionResult<SwingBotSettingsResponse>> UpdateSettings(
        Guid id,
        SwingBotSettingsRequest request,
        CancellationToken cancellationToken)
    {
        var settings = await swingBotService.UpsertSettingsAsync(id, request, cancellationToken);
        return settings is null ? NotFound() : Ok(settings);
    }

    [HttpGet("{id:guid}/dashboard")]
    public async Task<ActionResult<SwingBotDashboardResponse>> GetDashboard(
        Guid id,
        CancellationToken cancellationToken)
    {
        var dashboard = await swingBotService.GetDashboardAsync(id, cancellationToken);
        return dashboard is null ? NotFound() : Ok(dashboard);
    }

    [HttpGet("{id:guid}/logs")]
    public async Task<ActionResult<IReadOnlyList<BotLogResponse>>> GetLogs(
        Guid id,
        int count = 100,
        CancellationToken cancellationToken = default) =>
        Ok(await swingBotService.GetLogsAsync(id, count, cancellationToken));

    [HttpGet("{id:guid}/logs/stream")]
    public async Task StreamLogs(Guid id, CancellationToken cancellationToken)
    {
        Response.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        var latestLogId = 0L;

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var logs = await swingBotService.GetLogsAsync(id, 100, cancellationToken);
                foreach (var log in logs.Where(item => item.Id > latestLogId).OrderBy(item => item.Id))
                {
                    await Response.WriteAsync($"data: {JsonSerializer.Serialize(log)}\n\n", cancellationToken);
                    latestLogId = log.Id;
                }

                await Response.Body.FlushAsync(cancellationToken);
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Expected when the browser closes an EventSource connection.
        }
    }

    [HttpPost("{id:guid}/evaluate")]
    public async Task<ActionResult<BotEvaluationResponse>> Evaluate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await swingBotService.EvaluateAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
