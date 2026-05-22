using Mangefy.Domain.Common;
using Mangefy.Domain.Common.ValueObjects;
using Mangefy.Domain.Tenants.Events;

namespace Mangefy.Domain.Tenants;

public sealed class Tenant : AggregateRoot
{
    public Guid OwnerId { get; private set; }
    public string Name { get; private set; }
    public string Slug { get; private set; }
    public Email? Email { get; private set; }
    public PhoneNumber? Phone { get; private set; }
    public Address? Address { get; private set; }
    public string? LogoUrl { get; private set; }
    public TenantStatus Status { get; private set; }
    public Guid PlanId { get; private set; }
    public Guid BusinessTypeId { get; private set; }

    /// <summary>
    /// Fuso horário do estabelecimento no formato IANA (ex: "America/Sao_Paulo").
    /// Definido pelo AdminSaas na criação do tenant. Usado para verificar horário
    /// de funcionamento e turno dos funcionários em todas as operações.
    /// </summary>
    public string Timezone { get; private set; }

    public DateTime? TrialEndsAt { get; private set; }
    public DateTime? SuspendedAt { get; private set; }

    private Tenant() { }

    public static Tenant Create(
        Guid ownerId,
        string name,
        string slug,
        Guid planId,
        Guid businessTypeId,
        string timezone = "America/Sao_Paulo",
        int trialDays = 14,
        string? email = null)
    {
        if (ownerId == Guid.Empty)
            throw new DomainException("OwnerId inválido.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do estabelecimento não pode ser vazio.");

        if (string.IsNullOrWhiteSpace(slug))
            throw new DomainException("Slug não pode ser vazio.");

        if (!IsValidSlug(slug))
            throw new DomainException("Slug deve conter apenas letras minúsculas, números e hifens.");

        if (planId == Guid.Empty)
            throw new DomainException("Plano inválido.");

        if (businessTypeId == Guid.Empty)
            throw new DomainException("Tipo de negócio inválido.");

        if (string.IsNullOrWhiteSpace(timezone))
            throw new DomainException("Fuso horário não pode ser vazio.");

        var tenant = new Tenant
        {
            OwnerId = ownerId,
            Name = name.Trim(),
            Slug = slug.ToLowerInvariant().Trim(),
            Email = string.IsNullOrWhiteSpace(email) ? null : Email.Create(email),
            PlanId = planId,
            BusinessTypeId = businessTypeId,
            Timezone = timezone.Trim(),
            Status = TenantStatus.TrialPeriod,
            TrialEndsAt = DateTime.UtcNow.AddDays(trialDays)
        };

        tenant.AddDomainEvent(new TenantCreatedEvent(tenant.Id, tenant.Name, tenant.Slug, businessTypeId));
        return tenant;
    }

    public void UpdateInfo(string name, string? logoUrl = null, string? email = null, string? timezone = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do estabelecimento não pode ser vazio.");

        Name = name.Trim();
        LogoUrl = logoUrl;
        Email = string.IsNullOrWhiteSpace(email) ? null : Email.Create(email.Trim());
        if (!string.IsNullOrWhiteSpace(timezone))
            Timezone = timezone.Trim();
        SetUpdatedAt();
    }

    public void SetPhone(string phone)
    {
        Phone = PhoneNumber.Create(phone);
        SetUpdatedAt();
    }

    public void SetAddress(string cep, string logradouro, string numero,
        string bairro, string cidade, string uf, string? complemento = null)
    {
        Address = Address.Create(cep, logradouro, numero, bairro, cidade, uf, complemento);
        SetUpdatedAt();
    }

    public void Activate()
    {
        if (Status == TenantStatus.Cancelled)
            throw new DomainException("Não é possível reativar um tenant cancelado.");

        Status = TenantStatus.Active;
        SuspendedAt = null;
        SetUpdatedAt();
    }

    public void Suspend()
    {
        if (Status == TenantStatus.Cancelled)
            throw new DomainException("Tenant já está cancelado.");

        Status = TenantStatus.Suspended;
        SuspendedAt = DateTime.UtcNow;
        SetUpdatedAt();
        AddDomainEvent(new TenantSuspendedEvent(Id, Name));
    }

    public void Cancel()
    {
        Status = TenantStatus.Cancelled;
        SetUpdatedAt();
        AddDomainEvent(new TenantCancelledEvent(Id, Name));
    }

    public void ChangeBusinessType(Guid newBusinessTypeId)
    {
        if (newBusinessTypeId == Guid.Empty)
            throw new DomainException("Tipo de negócio inválido.");

        BusinessTypeId = newBusinessTypeId;
        SetUpdatedAt();
    }

    public void ChangePlan(Guid newPlanId)
    {
        if (newPlanId == Guid.Empty)
            throw new DomainException("Plano inválido.");

        var previousPlanId = PlanId;
        PlanId = newPlanId;
        SetUpdatedAt();
        AddDomainEvent(new TenantPlanChangedEvent(Id, previousPlanId, newPlanId));
    }

    public bool IsActive() => Status is TenantStatus.Active or TenantStatus.TrialPeriod;

    private static bool IsValidSlug(string slug) =>
        !string.IsNullOrEmpty(slug) && slug.All(c => char.IsLetterOrDigit(c) || c == '-');
}
