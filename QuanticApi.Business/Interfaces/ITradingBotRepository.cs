using QuanticApi.Business.Models;

namespace QuanticApi.Business.Interfaces;

public interface ITradingBotRepository
{
    Task<IReadOnlyList<TradingBot>> GetAllAsync(CancellationToken cancellationToken);
    Task<TradingBot?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(TradingBot bot, CancellationToken cancellationToken);
    void Remove(TradingBot bot);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
