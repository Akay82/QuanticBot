using Microsoft.EntityFrameworkCore;
using QuanticApi.Business.Models;
using QuanticApi.Data.Persistence.Configurations;

namespace QuanticApi.Data.Persistence;

public sealed class TradingDbContext(DbContextOptions<TradingDbContext> options) : DbContext(options)
{
    public DbSet<TradingBot> TradingBots => Set<TradingBot>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Instrument> Instruments => Set<Instrument>();
    public DbSet<PaperAccount> PaperAccounts => Set<PaperAccount>();
    public DbSet<Strategy> Strategies => Set<Strategy>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<PriceHistory> PriceHistory => Set<PriceHistory>();
    public DbSet<Signal> Signals => Set<Signal>();
    public DbSet<Trade> Trades => Set<Trade>();
    public DbSet<Watchlist> Watchlists => Set<Watchlist>();
    public DbSet<BotStrategySettings> BotStrategySettings => Set<BotStrategySettings>();
    public DbSet<BotPosition> BotPositions => Set<BotPosition>();
    public DbSet<BotLog> BotLogs => Set<BotLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TradingDbContext).Assembly);
        modelBuilder.ConfigureTradingSchema();
    }
}
