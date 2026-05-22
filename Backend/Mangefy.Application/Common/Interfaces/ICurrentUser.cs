namespace Mangefy.Application.Common.Interfaces;

/// <summary>
/// Provê o contexto do usuário autenticado na requisição atual.
/// </summary>
public interface ICurrentUser
{
    Guid? TenantId { get; }
    Guid? EmployeeId { get; }
    Guid? OwnerId { get; }
    bool IsAdminSaas { get; }
    IReadOnlyList<string> Permissions { get; }

    bool HasPermission(string permission);
}
