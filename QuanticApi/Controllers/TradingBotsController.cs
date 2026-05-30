using Microsoft.AspNetCore.Mvc;
using QuanticApi.Business.Contracts;
using QuanticApi.Business.Interfaces;

namespace QuanticApi.Controllers;

[ApiController]
[Route("api/trading-bots")]
public sealed class TradingBotsController(ITradingBotService service) : ControllerBase
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
        var bot = await service.StartAsync(id, cancellationToken);
        return bot is null ? NotFound() : Ok(bot);
    }

    [HttpPost("{id:guid}/stop")]
    public async Task<ActionResult<TradingBotResponse>> Stop(Guid id, CancellationToken cancellationToken)
    {
        var bot = await service.StopAsync(id, cancellationToken);
        return bot is null ? NotFound() : Ok(bot);
    }
}
