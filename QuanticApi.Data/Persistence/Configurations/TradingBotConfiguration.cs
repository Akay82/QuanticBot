using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuanticApi.Business.Models;

namespace QuanticApi.Data.Persistence.Configurations;

public sealed class TradingBotConfiguration : IEntityTypeConfiguration<TradingBot>
{
    public void Configure(EntityTypeBuilder<TradingBot> builder)
    {
        builder.ToTable("trading_bots");
        builder.HasKey(bot => bot.Id);

        builder.Property(bot => bot.Id).HasColumnName("id");
        builder.Property(bot => bot.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(bot => bot.Symbol).HasColumnName("symbol").HasMaxLength(30).IsRequired();
        builder.Property(bot => bot.Exchange).HasColumnName("exchange").HasMaxLength(50).IsRequired();
        builder.Property(bot => bot.Strategy).HasColumnName("strategy").HasMaxLength(100).IsRequired();
        builder.Property(bot => bot.Allocation).HasColumnName("allocation").HasPrecision(18, 8);
        builder.Property(bot => bot.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
        builder.Property(bot => bot.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(bot => bot.UpdatedAtUtc).HasColumnName("updated_at_utc");
    }
}
