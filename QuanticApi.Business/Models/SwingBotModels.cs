namespace QuanticApi.Business.Models;

public sealed class BotStrategySettings
{
    public Guid BotId { get; set; }
    public long AccountId { get; set; }
    public long InstrumentId { get; set; }
    public string Timeframe { get; set; } = "4h";
    public int FastEmaPeriod { get; set; } = 50;
    public int SlowEmaPeriod { get; set; } = 200;
    public int RsiPeriod { get; set; } = 14;
    public int AtrPeriod { get; set; } = 14;
    public decimal RsiEntryMin { get; set; } = 40;
    public decimal RsiEntryMax { get; set; } = 55;
    public decimal RsiExit { get; set; } = 70;
    public decimal AtrStopMultiplier { get; set; } = 1.5m;
    public decimal AtrTakeProfitMultiplier { get; set; } = 3m;
    public decimal RiskPercent { get; set; } = 1;
    public bool IsEnabled { get; set; } = true;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class BotPosition
{
    public Guid BotId { get; set; }
    public long PositionId { get; set; }
    public decimal EntryPrice { get; set; }
    public decimal Quantity { get; set; }
    public decimal StopLoss { get; set; }
    public decimal TakeProfit { get; set; }
    public DateTime OpenedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class BotLog : EntityBase
{
    public Guid BotId { get; set; }
    public string Level { get; set; } = "INFO";
    public string EventType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
