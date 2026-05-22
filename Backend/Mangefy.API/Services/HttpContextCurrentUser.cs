using Mangefy.Application.Common.Interfaces;
using System.Security.Claims;

namespace Mangefy.API.Services;

public sealed class HttpContextCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextCurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid? TenantId =>
        Guid.TryParse(User?.FindFirstValue("tenantId"), out var id) ? id : null;

    public Guid? OwnerId =>
        Guid.TryParse(User?.FindFirstValue("ownerId"), out var oid) ? oid : null;

    public Guid? EmployeeId =>
        OwnerId.HasValue
            ? null
            : Guid.TryParse(User?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    public bool IsAdminSaas =>
        User?.FindFirstValue("isAdminSaas") == "true";

    public IReadOnlyList<string> Permissions =>
        User?.FindAll("permission").Select(c => c.Value).ToList() ?? [];

    public bool HasPermission(string permission) =>
        Permissions.Contains(permission);
}
