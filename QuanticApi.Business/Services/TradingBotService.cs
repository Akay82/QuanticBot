using QuanticApi.Business.Contracts;
using QuanticApi.Business.Exceptions;
using QuanticApi.Business.Interfaces;
using QuanticApi.Business.Models;

namespace QuanticApi.Business.Services;

public sealed class TradingBotService(ITradingBotRepository repository) : ITradingBotService
{
    public async Task<IReadOnlyList<TradingBotResponse>> GetAllAsync(CancellationToken cancellationToken) =>
        (await repository.GetAllAsync(cancellationToken)).Select(ToResponse).ToList();

    public async Task<TradingBotResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var bot = await repository.GetByIdAsync(id, cancellationToken);
        return bot is null ? null : ToResponse(bot);
    }

    public async Task<TradingBotResponse> CreateAsync(
        CreateTradingBotRequest request,
        CancellationToken cancellationToken)
    {
        Validate(request.Name, request.Symbol, request.Exchange, request.Strategy, request.Allocation);

        var now = DateTimeOffset.UtcNow;
        var bot = new TradingBot
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Symbol = request.Symbol.Trim().ToUpperInvariant(),
            Exchange = request.Exchange.Trim().ToUpperInvariant(),
            Strategy = request.Strategy.Trim(),
            Allocation = request.Allocation,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        await repository.AddAsync(bot, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return ToResponse(bot);
    }

    public async Task<TradingBotResponse?> UpdateAsync(
        Guid id,
        UpdateTradingBotRequest request,
        CancellationToken cancellationToken)
    {
        Validate(request.Name, request.Symbol, request.Exchange, request.Strategy, request.Allocation);

        var bot = await repository.GetByIdAsync(id, cancellationToken);
        if (bot is null)
        {
            return null;
        }

        EnsureStopped(bot, "update");
        bot.Name = request.Name.Trim();
        bot.Symbol = request.Symbol.Trim().ToUpperInvariant();
        bot.Exchange = request.Exchange.Trim().ToUpperInvariant();
        bot.Strategy = request.Strategy.Trim();
        bot.Allocation = request.Allocation;
        bot.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await repository.SaveChangesAsync(cancellationToken);
        return ToResponse(bot);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var bot = await repository.GetByIdAsync(id, cancellationToken);
        if (bot is null)
        {
            return false;
        }

        EnsureStopped(bot, "delete");
        repository.Remove(bot);
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<TradingBotResponse?> StartAsync(Guid id, CancellationToken cancellationToken) =>
        ChangeStatusAsync(id, TradingBotStatus.Running, cancellationToken);

    public Task<TradingBotResponse?> StopAsync(Guid id, CancellationToken cancellationToken) =>
        ChangeStatusAsync(id, TradingBotStatus.Stopped, cancellationToken);

    private async Task<TradingBotResponse?> ChangeStatusAsync(
        Guid id,
        TradingBotStatus status,
        CancellationToken cancellationToken)
    {
        var bot = await repository.GetByIdAsync(id, cancellationToken);
        if (bot is null)
        {
            return null;
        }

        bot.Status = status;
        bot.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await repository.SaveChangesAsync(cancellationToken);
        return ToResponse(bot);
    }

    private static void EnsureStopped(TradingBot bot, string operation)
    {
        if (bot.Status == TradingBotStatus.Running)
        {
            throw new BusinessRuleException($"Stop the bot before attempting to {operation} it.");
        }
    }

    private static void Validate(string name, string symbol, string exchange, string strategy, decimal allocation)
    {
        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(symbol) ||
            string.IsNullOrWhiteSpace(exchange) ||
            string.IsNullOrWhiteSpace(strategy))
        {
            throw new BusinessRuleException("Name, symbol, exchange, and strategy are required.");
        }

        if (allocation <= 0)
        {
            throw new BusinessRuleException("Allocation must be greater than zero.");
        }
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
}
