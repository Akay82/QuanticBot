using Microsoft.AspNetCore.Mvc;
using QuanticApi.Business.Contracts;
using QuanticApi.Business.Interfaces;
using QuanticApi.Business.Models;

namespace QuanticApi.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController(ICrudService<User> service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<UserResponse>>> GetAll(
        [FromQuery] PageRequest page,
        string? email,
        bool? isActive,
        CancellationToken cancellationToken)
    {
        var users = await service.GetPageAsync(
            page,
            item => (email == null || item.Email.Contains(email)) &&
                    (isActive == null || item.IsActive == isActive),
            cancellationToken);

        return Ok(new PagedResponse<UserResponse>(
            users.Items.Select(ToResponse).ToList(),
            users.PageNumber,
            users.PageSize,
            users.TotalCount,
            users.TotalPages));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<UserResponse>> GetById(long id, CancellationToken cancellationToken)
    {
        var user = await service.GetByIdAsync(id, cancellationToken);
        return user is null ? NotFound() : Ok(ToResponse(user));
    }

    [HttpPost]
    public async Task<ActionResult<UserResponse>> Create(UserRequest request, CancellationToken cancellationToken)
    {
        var user = await service.CreateAsync(ToEntity(request), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, ToResponse(user));
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<UserResponse>> Update(
        long id,
        UserRequest request,
        CancellationToken cancellationToken)
    {
        var user = await service.UpdateAsync(id, ToEntity(request), cancellationToken);
        return user is null ? NotFound() : Ok(ToResponse(user));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken) =>
        await service.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();

    private static User ToEntity(UserRequest request) =>
        new()
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = request.PasswordHash,
            IsActive = request.IsActive,
            CreatedAt = request.CreatedAt ?? DateTime.UtcNow
        };

    private static UserResponse ToResponse(User user) =>
        new(user.Id, user.FullName, user.Email, user.IsActive, user.CreatedAt);
}

public sealed record UserRequest(
    string? FullName,
    string Email,
    string PasswordHash,
    bool IsActive = true,
    DateTime? CreatedAt = null);

public sealed record UserResponse(long Id, string? FullName, string Email, bool IsActive, DateTime CreatedAt);
