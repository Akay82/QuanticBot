namespace QuanticApi.Business.Models;

public sealed class TradingBot
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string Exchange { get; set; } = string.Empty;
    public string Strategy { get; set; } = string.Empty;
    public decimal Allocation { get; set; }
    public TradingBotStatus Status { get; set; } = TradingBotStatus.Stopped;
    public DateTime? LastEvaluatedCandleUtc { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
}
