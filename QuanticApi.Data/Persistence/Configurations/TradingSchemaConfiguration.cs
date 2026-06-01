using Microsoft.EntityFrameworkCore;
using QuanticApi.Business.Models;

namespace QuanticApi.Data.Persistence.Configurations;

public static class TradingSchemaConfiguration
{
    public static void ConfigureTradingSchema(this ModelBuilder modelBuilder)
    {
        ConfigureUser(modelBuilder);
        ConfigureInstrument(modelBuilder);
        ConfigurePaperAccount(modelBuilder);
        ConfigureStrategy(modelBuilder);
        ConfigureOrder(modelBuilder);
        ConfigurePosition(modelBuilder);
        ConfigurePriceHistory(modelBuilder);
        ConfigureSignal(modelBuilder);
        ConfigureTrade(modelBuilder);
        ConfigureWatchlist(modelBuilder);
    }

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<User>();
        entity.ToTable("users", "quantic");
        entity.HasKey(item => item.Id);
        entity.Property(item => item.Id).HasColumnName("user_id");
        entity.Property(item => item.FullName).HasColumnName("full_name").HasMaxLength(150);
        entity.Property(item => item.Email).HasColumnName("email").HasMaxLength(150).IsRequired();
        entity.Property(item => item.PasswordHash).HasColumnName("password_hash").IsRequired();
        entity.Property(item => item.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        entity.Property(item => item.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.HasIndex(item => item.Email).IsUnique();
    }

    private static void ConfigureInstrument(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Instrument>();
        entity.ToTable("instruments", "quantic");
        entity.HasKey(item => item.Id);
        entity.Property(item => item.Id).HasColumnName("instrument_id");
        entity.Property(item => item.Symbol).HasColumnName("symbol").HasMaxLength(50).IsRequired();
        entity.Property(item => item.Name).HasColumnName("name").HasMaxLength(150);
        entity.Property(item => item.MarketType).HasColumnName("market_type").HasMaxLength(50).IsRequired();
        entity.Property(item => item.Exchange).HasColumnName("exchange").HasMaxLength(100);
        entity.Property(item => item.BaseCurrency).HasColumnName("base_currency").HasMaxLength(20);
        entity.Property(item => item.QuoteCurrency).HasColumnName("quote_currency").HasMaxLength(20);
        entity.Property(item => item.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        entity.Property(item => item.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
    }

    private static void ConfigurePaperAccount(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<PaperAccount>();
        entity.ToTable("paper_accounts", "quantic");
        entity.HasKey(item => item.Id);
        entity.Property(item => item.Id).HasColumnName("account_id");
        entity.Property(item => item.UserId).HasColumnName("user_id");
        entity.Property(item => item.AccountName).HasColumnName("account_name").HasMaxLength(100).IsRequired();
        entity.Property(item => item.StartingBalance).HasColumnName("starting_balance").HasPrecision(18, 4).HasDefaultValue(10000m);
        entity.Property(item => item.CurrentBalance).HasColumnName("current_balance").HasPrecision(18, 4).HasDefaultValue(10000m);
        entity.Property(item => item.Currency).HasColumnName("currency").HasMaxLength(20).HasDefaultValue("USD");
        entity.Property(item => item.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        entity.Property(item => item.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.HasOne<User>().WithMany().HasForeignKey(item => item.UserId);
    }

    private static void ConfigureStrategy(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Strategy>();
        entity.ToTable("strategies", "quantic");
        entity.HasKey(item => item.Id);
        entity.Property(item => item.Id).HasColumnName("strategy_id");
        entity.Property(item => item.UserId).HasColumnName("user_id");
        entity.Property(item => item.StrategyName).HasColumnName("strategy_name").HasMaxLength(150).IsRequired();
        entity.Property(item => item.Description).HasColumnName("description");
        entity.Property(item => item.StrategyType).HasColumnName("strategy_type").HasMaxLength(100);
        entity.Property(item => item.Parameters).HasColumnName("parameters").HasColumnType("jsonb");
        entity.Property(item => item.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        entity.Property(item => item.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.HasOne<User>().WithMany().HasForeignKey(item => item.UserId);
    }

    private static void ConfigureOrder(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Order>();
        entity.ToTable("orders", "quantic");
        entity.HasKey(item => item.Id);
        entity.Property(item => item.Id).HasColumnName("order_id");
        entity.Property(item => item.AccountId).HasColumnName("account_id");
        entity.Property(item => item.StrategyId).HasColumnName("strategy_id");
        entity.Property(item => item.InstrumentId).HasColumnName("instrument_id");
        entity.Property(item => item.OrderType).HasColumnName("order_type").HasMaxLength(30).IsRequired();
        entity.Property(item => item.Side).HasColumnName("side").HasMaxLength(10).IsRequired();
        entity.Property(item => item.Quantity).HasColumnName("quantity").HasPrecision(18, 6);
        entity.Property(item => item.RequestedPrice).HasColumnName("requested_price").HasPrecision(18, 6);
        entity.Property(item => item.ExecutedPrice).HasColumnName("executed_price").HasPrecision(18, 6);
        entity.Property(item => item.Status).HasColumnName("status").HasMaxLength(30).HasDefaultValue("PENDING");
        entity.Property(item => item.OrderTime).HasColumnName("order_time").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(item => item.ExecutedTime).HasColumnName("executed_time");
        entity.HasOne<PaperAccount>().WithMany().HasForeignKey(item => item.AccountId);
        entity.HasOne<Strategy>().WithMany().HasForeignKey(item => item.StrategyId);
        entity.HasOne<Instrument>().WithMany().HasForeignKey(item => item.InstrumentId);
    }

    private static void ConfigurePosition(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Position>();
        entity.ToTable("positions", "quantic");
        entity.HasKey(item => item.Id);
        entity.Property(item => item.Id).HasColumnName("position_id");
        entity.Property(item => item.AccountId).HasColumnName("account_id");
        entity.Property(item => item.InstrumentId).HasColumnName("instrument_id");
        entity.Property(item => item.Quantity).HasColumnName("quantity").HasPrecision(18, 6).HasDefaultValue(0m);
        entity.Property(item => item.AveragePrice).HasColumnName("average_price").HasPrecision(18, 6).HasDefaultValue(0m);
        entity.Property(item => item.CurrentPrice).HasColumnName("current_price").HasPrecision(18, 6).HasDefaultValue(0m);
        entity.Property(item => item.UnrealizedPnl).HasColumnName("unrealized_pnl").HasPrecision(18, 6).HasDefaultValue(0m);
        entity.Property(item => item.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.HasIndex(item => new { item.AccountId, item.InstrumentId }).IsUnique();
        entity.HasOne<PaperAccount>().WithMany().HasForeignKey(item => item.AccountId);
        entity.HasOne<Instrument>().WithMany().HasForeignKey(item => item.InstrumentId);
    }

    private static void ConfigurePriceHistory(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<PriceHistory>();
        entity.ToTable("price_history", "quantic");
        entity.HasKey(item => item.Id);
        entity.Property(item => item.Id).HasColumnName("price_id");
        entity.Property(item => item.InstrumentId).HasColumnName("instrument_id");
        entity.Property(item => item.Timeframe).HasColumnName("timeframe").HasMaxLength(20).IsRequired();
        entity.Property(item => item.OpenPrice).HasColumnName("open_price").HasPrecision(18, 6);
        entity.Property(item => item.HighPrice).HasColumnName("high_price").HasPrecision(18, 6);
        entity.Property(item => item.LowPrice).HasColumnName("low_price").HasPrecision(18, 6);
        entity.Property(item => item.ClosePrice).HasColumnName("close_price").HasPrecision(18, 6);
        entity.Property(item => item.Volume).HasColumnName("volume").HasPrecision(18, 6);
        entity.Property(item => item.CandleTime).HasColumnName("candle_time");
        entity.HasIndex(item => new { item.InstrumentId, item.Timeframe, item.CandleTime }).IsUnique();
        entity.HasOne<Instrument>().WithMany().HasForeignKey(item => item.InstrumentId);
    }

    private static void ConfigureSignal(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Signal>();
        entity.ToTable("signals", "quantic");
        entity.HasKey(item => item.Id);
        entity.Property(item => item.Id).HasColumnName("signal_id");
        entity.Property(item => item.StrategyId).HasColumnName("strategy_id");
        entity.Property(item => item.InstrumentId).HasColumnName("instrument_id");
        entity.Property(item => item.SignalType).HasColumnName("signal_type").HasMaxLength(20).IsRequired();
        entity.Property(item => item.SignalPrice).HasColumnName("signal_price").HasPrecision(18, 6);
        entity.Property(item => item.Confidence).HasColumnName("confidence").HasPrecision(5, 2);
        entity.Property(item => item.Reason).HasColumnName("reason");
        entity.Property(item => item.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.HasOne<Strategy>().WithMany().HasForeignKey(item => item.StrategyId);
        entity.HasOne<Instrument>().WithMany().HasForeignKey(item => item.InstrumentId);
    }

    private static void ConfigureTrade(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Trade>();
        entity.ToTable("trades", "quantic");
        entity.HasKey(item => item.Id);
        entity.Property(item => item.Id).HasColumnName("trade_id");
        entity.Property(item => item.OrderId).HasColumnName("order_id");
        entity.Property(item => item.AccountId).HasColumnName("account_id");
        entity.Property(item => item.InstrumentId).HasColumnName("instrument_id");
        entity.Property(item => item.Side).HasColumnName("side").HasMaxLength(10).IsRequired();
        entity.Property(item => item.Quantity).HasColumnName("quantity").HasPrecision(18, 6);
        entity.Property(item => item.Price).HasColumnName("price").HasPrecision(18, 6);
        entity.Property(item => item.TotalValue).HasColumnName("total_value").HasPrecision(18, 6);
        entity.Property(item => item.ProfitLoss).HasColumnName("profit_loss").HasPrecision(18, 6).HasDefaultValue(0m);
        entity.Property(item => item.TradeTime).HasColumnName("trade_time").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.HasOne<Order>().WithMany().HasForeignKey(item => item.OrderId);
        entity.HasOne<PaperAccount>().WithMany().HasForeignKey(item => item.AccountId);
        entity.HasOne<Instrument>().WithMany().HasForeignKey(item => item.InstrumentId);
    }

    private static void ConfigureWatchlist(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Watchlist>();
        entity.ToTable("watchlists", "quantic");
        entity.HasKey(item => item.Id);
        entity.Property(item => item.Id).HasColumnName("watchlist_id");
        entity.Property(item => item.UserId).HasColumnName("user_id");
        entity.Property(item => item.InstrumentId).HasColumnName("instrument_id");
        entity.Property(item => item.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.HasIndex(item => new { item.UserId, item.InstrumentId }).IsUnique();
        entity.HasOne<User>().WithMany().HasForeignKey(item => item.UserId);
        entity.HasOne<Instrument>().WithMany().HasForeignKey(item => item.InstrumentId);
    }
}
