namespace QuanticApi.Business.Models;

public sealed class User : EntityBase
{
    public string? FullName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class Instrument : EntityBase
{
    public string Symbol { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string MarketType { get; set; } = string.Empty;
    public string? Exchange { get; set; }
    public string? BaseCurrency { get; set; }
    public string? QuoteCurrency { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class PaperAccount : EntityBase
{
    public long UserId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public decimal StartingBalance { get; set; } = 10000;
    public decimal CurrentBalance { get; set; } = 10000;
    public string Currency { get; set; } = "USD";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class Strategy : EntityBase
{
    public long UserId { get; set; }
    public string StrategyName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? StrategyType { get; set; }
    public string? Parameters { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class Order : EntityBase
{
    public long AccountId { get; set; }
    public long? StrategyId { get; set; }
    public long InstrumentId { get; set; }
    public string OrderType { get; set; } = string.Empty;
    public string Side { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal? RequestedPrice { get; set; }
    public decimal? ExecutedPrice { get; set; }
    public string Status { get; set; } = "PENDING";
    public DateTime OrderTime { get; set; } = DateTime.UtcNow;
    public DateTime? ExecutedTime { get; set; }
}

public sealed class Position : EntityBase
{
    public long AccountId { get; set; }
    public long InstrumentId { get; set; }
    public decimal Quantity { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal? CurrentPrice { get; set; }
    public decimal? UnrealizedPnl { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class PriceHistory : EntityBase
{
    public long InstrumentId { get; set; }
    public string Timeframe { get; set; } = string.Empty;
    public decimal OpenPrice { get; set; }
    public decimal HighPrice { get; set; }
    public decimal LowPrice { get; set; }
    public decimal ClosePrice { get; set; }
    public decimal? Volume { get; set; }
    public DateTime CandleTime { get; set; }
}

public sealed class Signal : EntityBase
{
    public long StrategyId { get; set; }
    public long InstrumentId { get; set; }
    public string SignalType { get; set; } = string.Empty;
    public decimal? SignalPrice { get; set; }
    public decimal? Confidence { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class Trade : EntityBase
{
    public long OrderId { get; set; }
    public long AccountId { get; set; }
    public long InstrumentId { get; set; }
    public string Side { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal TotalValue { get; set; }
    public decimal? ProfitLoss { get; set; }
    public DateTime TradeTime { get; set; } = DateTime.UtcNow;
}

public sealed class Watchlist : EntityBase
{
    public long UserId { get; set; }
    public long InstrumentId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
