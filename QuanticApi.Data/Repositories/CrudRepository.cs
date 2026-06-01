using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using QuanticApi.Business.Contracts;
using QuanticApi.Business.Interfaces;
using QuanticApi.Business.Models;
using QuanticApi.Data.Persistence;

namespace QuanticApi.Data.Repositories;

public sealed class CrudRepository<TEntity>(TradingDbContext dbContext) : ICrudRepository<TEntity>
    where TEntity : EntityBase
{
    public async Task<PagedResponse<TEntity>> GetPageAsync(
        PageRequest request,
        Expression<Func<TEntity, bool>>? filter,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Set<TEntity>().AsNoTracking();
        if (filter is not null)
        {
            query = query.Where(filter);
        }

        var pageNumber = request.SafePageNumber;
        var pageSize = request.SafePageSize;
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(entity => entity.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResponse<TEntity>(
            items,
            pageNumber,
            pageSize,
            totalCount,
            (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public Task<TEntity?> GetByIdAsync(long id, CancellationToken cancellationToken) =>
        dbContext.Set<TEntity>().AsNoTracking().SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);

    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken)
    {
        dbContext.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<TEntity?> UpdateAsync(long id, TEntity entity, CancellationToken cancellationToken)
    {
        var existing = await dbContext.Set<TEntity>().SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        entity.Id = id;
        dbContext.Entry(existing).CurrentValues.SetValues(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Set<TEntity>().SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        dbContext.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
