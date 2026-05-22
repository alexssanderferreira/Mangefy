namespace Mangefy.Application.Common.Interfaces;

/// <summary>
/// Verifica se um funcionário pode operar com base em seu turno e acesso temporário.
/// Owner sempre passa (acesso irrestrito 24/7).
/// </summary>
public interface IShiftEnforcementService
{
    /// <summary>
    /// Verifica se o funcionário está em turno ou possui acesso temporário.
    /// Retorna true se pode operar, false caso contrário.
    /// </summary>
    Task<bool> CanOperateAsync(Guid tenantId, Guid employeeId, CancellationToken ct = default);

    /// <summary>
    /// Lança <see cref="Exceptions.ForbiddenException"/> se o funcionário não puder operar.
    /// </summary>
    Task EnsureCanOperateAsync(Guid tenantId, Guid employeeId, CancellationToken ct = default);
}
