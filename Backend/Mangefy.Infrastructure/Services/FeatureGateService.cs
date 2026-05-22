using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Platform.Features.Repositories;
using Mangefy.Domain.Tenants.Repositories;

namespace Mangefy.Infrastructure.Services;

public sealed class FeatureGateService : IFeatureGateService
{
    private readonly ITenantRepository _tenants;
    private readonly IPlanFeatureSetRepository _planFeatureSets;
    private readonly IFeatureGracePeriodRepository _gracePeriods;

    public FeatureGateService(
        ITenantRepository tenants,
        IPlanFeatureSetRepository planFeatureSets,
        IFeatureGracePeriodRepository gracePeriods)
    {
        _tenants = tenants;
        _planFeatureSets = planFeatureSets;
        _gracePeriods = gracePeriods;
    }

    public async Task<bool> IsEnabledAsync(Guid tenantId, string featureKey, CancellationToken ct = default)
    {
        var tenant = await _tenants.GetByIdAsync(tenantId, ct);
        if (tenant is null) return false;

        var featureSet = await _planFeatureSets.GetByPlanAndBusinessTypeAsync(
            tenant.PlanId, tenant.BusinessTypeId, ct);

        if (featureSet is not null && featureSet.HasFeature(featureKey))
            return true;

        // Verificar se está dentro do período de carência
        var grace = await _gracePeriods.GetByTenantAndFeatureAsync(tenantId, featureKey, ct);
        return grace is not null && grace.IsActive();
    }

    public async Task RequireAsync(Guid tenantId, string featureKey, CancellationToken ct = default)
    {
        if (!await IsEnabledAsync(tenantId, featureKey, ct))
            throw new ForbiddenException($"Recurso '{featureKey}' não disponível no plano atual.");
    }
}
