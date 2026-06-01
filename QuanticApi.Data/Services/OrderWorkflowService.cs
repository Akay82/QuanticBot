using Microsoft.EntityFrameworkCore;
using QuanticApi.Business.Interfaces;
using QuanticApi.Data.Persistence;

namespace QuanticApi.Data.Services;

public sealed class OrderWorkflowService(TradingDbContext dbContext) : IOrderWorkflowService
{
    public Task CancelAsync(long orderId, CancellationToken cancellationToken) =>
        dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"CALL quantic.cancel_order({orderId})",
            cancellationToken);

    public Task ExecuteAsync(long orderId, decimal executedPrice, CancellationToken cancellationToken) =>
        dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"CALL quantic.execute_order({orderId}, {executedPrice})",
            cancellationToken);
}
