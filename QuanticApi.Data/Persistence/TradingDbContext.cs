using Microsoft.EntityFrameworkCore;
using QuanticApi.Business.Models;

namespace QuanticApi.Data.Persistence;

public sealed class TradingDbContext(DbContextOptions<TradingDbContext> options) : DbContext(options)
{
    public DbSet<TradingBot> TradingBots => Set<TradingBot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TradingDbContext).Assembly);
    }
}
