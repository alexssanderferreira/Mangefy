using Mangefy.Application.Common.Exceptions;
using Mangefy.Domain.Owners;
using Mangefy.Domain.Owners.Repositories;
using Mangefy.Domain.Plans.Repositories;
using Mangefy.Domain.Tenants;
using Mangefy.Domain.Tenants.Repositories;
using MediatR;

namespace Mangefy.Application.Owners.Queries.GetOwnerById;

public sealed class GetOwnerByIdQueryHandler : IRequestHandler<GetOwnerByIdQuery, OwnerDetailDto>
{
    private readonly IOwnerRepository _owners;
    private readonly ITenantRepository _tenants;
    private readonly IPlanRepository _plans;

    public GetOwnerByIdQueryHandler(IOwnerRepository owners, ITenantRepository tenants, IPlanRepository plans)
    {
        _owners = owners;
        _tenants = tenants;
        _plans = plans;
    }

    public async Task<OwnerDetailDto> Handle(GetOwnerByIdQuery request, CancellationToken cancellationToken)
    {
        var owner = await _owners.GetByIdAsync(request.OwnerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Owner), request.OwnerId);

        var tenants = await _tenants.GetByOwnerAsync(request.OwnerId, cancellationToken);

        var planIds = tenants.Select(t => t.PlanId).Distinct().ToList();
        var allPlans = await _plans.GetAllAsync(cancellationToken);
        var planMap = allPlans.Where(p => planIds.Contains(p.Id)).ToDictionary(p => p.Id);

        var tenantDtos = tenants.Select(t =>
        {
            planMap.TryGetValue(t.PlanId, out var plan);
            return new OwnerTenantDto(t.Id, t.Name, t.Slug, t.Status.ToString(), plan?.Name, plan?.MonthlyPrice.Amount);
        }).ToList();

        var activeOrTrial = tenants.Where(t => t.Status == TenantStatus.Active || t.Status == TenantStatus.TrialPeriod).ToList();
        var mrr = activeOrTrial.Sum(t => planMap.TryGetValue(t.PlanId, out var p) ? p.MonthlyPrice.Amount : 0m);
        var distinctPlanNames = activeOrTrial.Select(t => planMap.TryGetValue(t.PlanId, out var p) ? p.Name : null)
            .Where(n => n is not null).Distinct().Cast<string>().ToList();

        var metrics = new OwnerMetricsDto(
            TotalEstablishments: tenants.Count,
            ActiveEstablishments: tenants.Count(t => t.Status == TenantStatus.Active),
            TrialEstablishments: tenants.Count(t => t.Status == TenantStatus.TrialPeriod),
            SuspendedEstablishments: tenants.Count(t => t.Status == TenantStatus.Suspended),
            Plans: distinctPlanNames,
            EstimatedMrr: mrr,
            DaysAsClient: (int)(DateTime.UtcNow - owner.CreatedAt).TotalDays);

        OwnerAddressDto? addressDto = owner.Address is null ? null : new OwnerAddressDto(
            owner.Address.Cep, owner.Address.Logradouro, owner.Address.Numero,
            owner.Address.Complemento, owner.Address.Bairro, owner.Address.Cidade, owner.Address.Uf);

        return new OwnerDetailDto(
            owner.Id,
            owner.Name,
            owner.Email.Value,
            owner.Phone?.Value,
            owner.DocumentType?.ToString(),
            owner.DocumentNumber,
            owner.Notes,
            addressDto,
            owner.Status.ToString(),
            owner.LastLoginAt,
            owner.CreatedAt,
            metrics,
            tenantDtos);
    }
}
