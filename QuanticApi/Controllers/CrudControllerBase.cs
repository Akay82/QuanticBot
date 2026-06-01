using Microsoft.AspNetCore.Mvc;
using QuanticApi.Business.Contracts;
using QuanticApi.Business.Interfaces;
using QuanticApi.Business.Models;

namespace QuanticApi.Controllers;

[ApiController]
public abstract class CrudControllerBase<TEntity>(ICrudService<TEntity> service) : ControllerBase
    where TEntity : EntityBase
{
    protected ICrudService<TEntity> Service { get; } = service;

    protected async Task<ActionResult<PagedResponse<TEntity>>> GetPage(
        PageRequest page,
        System.Linq.Expressions.Expression<Func<TEntity, bool>>? filter,
        CancellationToken cancellationToken) =>
        Ok(await Service.GetPageAsync(page, filter, cancellationToken));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<TEntity>> GetById(long id, CancellationToken cancellationToken)
    {
        var entity = await Service.GetByIdAsync(id, cancellationToken);
        return entity is null ? NotFound() : Ok(entity);
    }

    [HttpPost]
    public async Task<ActionResult<TEntity>> Create(TEntity entity, CancellationToken cancellationToken)
    {
        var created = await Service.CreateAsync(entity, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<TEntity>> Update(long id, TEntity entity, CancellationToken cancellationToken)
    {
        var updated = await Service.UpdateAsync(id, entity, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken) =>
        await Service.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
}
