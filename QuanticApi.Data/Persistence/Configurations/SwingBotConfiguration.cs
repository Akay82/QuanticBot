using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuanticApi.Business.Models;

namespace QuanticApi.Data.Persistence.Configurations;

public sealed class BotStrategySettingsConfiguration : IEntityTypeConfiguration<BotStrategySettings>
{
    public void Configure(EntityTypeBuilder<BotStrategySettings> builder)
    {
        builder.ToTable("bot_strategy_settings", "quantic");
        builder.HasKey(item => item.BotId);
        builder.Property(item => item.BotId).HasColumnName("bot_id");
        builder.Property(item => item.AccountId).HasColumnName("account_id");
        builder.Property(item => item.InstrumentId).HasColumnName("instrument_id");
        builder.Property(item => item.Timeframe).HasColumnName("timeframe").HasMaxLength(10);
        builder.Property(item => item.FastEmaPeriod).HasColumnName("fast_ema_period");
        builder.Property(item => item.SlowEmaPeriod).HasColumnName("slow_ema_period");
        builder.Property(item => item.RsiPeriod).HasColumnName("rsi_period");
        builder.Property(item => item.AtrPeriod).HasColumnName("atr_period");
        builder.Property(item => item.RsiEntryMin).HasColumnName("rsi_entry_min").HasPrecision(5, 2);
        builder.Property(item => item.RsiEntryMax).HasColumnName("rsi_entry_max").HasPrecision(5, 2);
        builder.Property(item => item.RsiExit).HasColumnName("rsi_exit").HasPrecision(5, 2);
        builder.Property(item => item.AtrStopMultiplier).HasColumnName("atr_stop_multiplier").HasPrecision(8, 4);
        builder.Property(item => item.AtrTakeProfitMultiplier).HasColumnName("atr_take_profit_multiplier").HasPrecision(8, 4);
        builder.Property(item => item.RiskPercent).HasColumnName("risk_percent").HasPrecision(5, 2);
        builder.Property(item => item.IsEnabled).HasColumnName("is_enabled");
        builder.Property(item => item.UpdatedAtUtc).HasColumnName("updated_at_utc");
        builder.HasOne<TradingBot>().WithOne().HasForeignKey<BotStrategySettings>(item => item.BotId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<PaperAccount>().WithMany().HasForeignKey(item => item.AccountId);
        builder.HasOne<Instrument>().WithMany().HasForeignKey(item => item.InstrumentId);
    }
}

public sealed class BotPositionConfiguration : IEntityTypeConfiguration<BotPosition>
{
    public void Configure(EntityTypeBuilder<BotPosition> builder)
    {
        builder.ToTable("bot_positions", "quantic");
        builder.HasKey(item => item.BotId);
        builder.Property(item => item.BotId).HasColumnName("bot_id");
        builder.Property(item => item.PositionId).HasColumnName("position_id");
        builder.Property(item => item.EntryPrice).HasColumnName("entry_price").HasPrecision(18, 6);
        builder.Property(item => item.Quantity).HasColumnName("quantity").HasPrecision(18, 6);
        builder.Property(item => item.StopLoss).HasColumnName("stop_loss").HasPrecision(18, 6);
        builder.Property(item => item.TakeProfit).HasColumnName("take_profit").HasPrecision(18, 6);
        builder.Property(item => item.OpenedAtUtc).HasColumnName("opened_at_utc");
        builder.Property(item => item.UpdatedAtUtc).HasColumnName("updated_at_utc");
        builder.HasOne<TradingBot>().WithOne().HasForeignKey<BotPosition>(item => item.BotId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Position>().WithMany().HasForeignKey(item => item.PositionId);
    }
}

public sealed class BotLogConfiguration : IEntityTypeConfiguration<BotLog>
{
    public void Configure(EntityTypeBuilder<BotLog> builder)
    {
        builder.ToTable("bot_logs", "quantic");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).HasColumnName("log_id");
        builder.Property(item => item.BotId).HasColumnName("bot_id");
        builder.Property(item => item.Level).HasColumnName("level").HasMaxLength(20);
        builder.Property(item => item.EventType).HasColumnName("event_type").HasMaxLength(50);
        builder.Property(item => item.Message).HasColumnName("message").HasMaxLength(500);
        builder.Property(item => item.Details).HasColumnName("details").HasColumnType("jsonb");
        builder.Property(item => item.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.HasOne<TradingBot>().WithMany().HasForeignKey(item => item.BotId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(item => new { item.BotId, item.CreatedAtUtc });
    }
}
