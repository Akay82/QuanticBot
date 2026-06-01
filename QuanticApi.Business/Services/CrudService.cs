using System.Linq.Expressions;
using QuanticApi.Business.Contracts;
using QuanticApi.Business.Interfaces;
using QuanticApi.Business.Models;

namespace QuanticApi.Business.Services;

public sealed class CrudService<TEntity>(ICrudRepository<TEntity> repository) : ICrudService<TEntity>
    where TEntity : EntityBase
{
    public Task<PagedResponse<TEntity>> GetPageAsync(
        PageRequest request,
        Expression<Func<TEntity, bool>>? filter,
        CancellationToken cancellationToken) =>
        repository.GetPageAsync(request, filter, cancellationToken);

    public Task<TEntity?> GetByIdAsync(long id, CancellationToken cancellationToken) =>
        repository.GetByIdAsync(id, cancellationToken);

    public Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken)
    {
        entity.Id = 0;
        return repository.AddAsync(entity, cancellationToken);
    }

    public Task<TEntity?> UpdateAsync(long id, TEntity entity, CancellationToken cancellationToken) =>
        repository.UpdateAsync(id, entity, cancellationToken);

    public Task<bool> DeleteAsync(long id, CancellationToken cancellationToken) =>
        repository.DeleteAsync(id, cancellationToken);
}
