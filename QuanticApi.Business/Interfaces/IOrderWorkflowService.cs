namespace QuanticApi.Business.Interfaces;

public interface IOrderWorkflowService
{
    Task CancelAsync(long orderId, CancellationToken cancellationToken);
    Task ExecuteAsync(long orderId, decimal executedPrice, CancellationToken cancellationToken);
}
