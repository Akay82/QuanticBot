using Microsoft.EntityFrameworkCore;
using QuanticApi.Business.Interfaces;
using QuanticApi.Business.Models;
using QuanticApi.Data.Persistence;

namespace QuanticApi.Data.Repositories;

public sealed class TradingBotRepository(TradingDbContext dbContext) : ITradingBotRepository
{
    public async Task<IReadOnlyList<TradingBot>> GetAllAsync(CancellationToken cancellationToken) =>
        await dbContext.TradingBots
            .AsNoTracking()
            .OrderBy(bot => bot.Name)
            .ToListAsync(cancellationToken);

    public Task<TradingBot?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.TradingBots.SingleOrDefaultAsync(bot => bot.Id == id, cancellationToken);

    public Task AddAsync(TradingBot bot, CancellationToken cancellationToken) =>
        dbContext.TradingBots.AddAsync(bot, cancellationToken).AsTask();

    public void Remove(TradingBot bot) => dbContext.TradingBots.Remove(bot);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
