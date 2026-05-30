using QuanticApi.Business.Contracts;

namespace QuanticApi.Business.Interfaces;

public interface ITradingBotService
{
    Task<IReadOnlyList<TradingBotResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<TradingBotResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<TradingBotResponse> CreateAsync(CreateTradingBotRequest request, CancellationToken cancellationToken);
    Task<TradingBotResponse?> UpdateAsync(Guid id, UpdateTradingBotRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<TradingBotResponse?> StartAsync(Guid id, CancellationToken cancellationToken);
    Task<TradingBotResponse?> StopAsync(Guid id, CancellationToken cancellationToken);
}
