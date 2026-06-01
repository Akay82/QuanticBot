using System.Linq.Expressions;
using QuanticApi.Business.Contracts;
using QuanticApi.Business.Models;

namespace QuanticApi.Business.Interfaces;

public interface ICrudService<TEntity> where TEntity : EntityBase
{
    Task<PagedResponse<TEntity>> GetPageAsync(
        PageRequest request,
        Expression<Func<TEntity, bool>>? filter,
        CancellationToken cancellationToken);

    Task<TEntity?> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken);
    Task<TEntity?> UpdateAsync(long id, TEntity entity, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken);
}
