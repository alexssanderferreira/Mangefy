namespace Mangefy.Application.Common.Interfaces;

/// <summary>
/// Verifica se um tenant tem acesso a uma feature do plano, respeitando períodos de carência.
/// </summary>
public interface IFeatureGateService
{
    /// <summary>
    /// Retorna true se o tenant tem acesso à feature (plano ativo ou dentro do período de carência).
    /// </summary>
    Task<bool> IsEnabledAsync(Guid tenantId, string featureKey, CancellationToken ct = default);

    /// <summary>
    /// Lança ForbiddenException se o tenant não tem acesso à feature.
    /// </summary>
    Task RequireAsync(Guid tenantId, string featureKey, CancellationToken ct = default);
}
