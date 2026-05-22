using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.BusinessSchedules;
using Mangefy.Domain.BusinessSchedules.Repositories;
using Mangefy.Domain.Common;
using Mangefy.Domain.Menus;
using Mangefy.Domain.Menus.Repositories;
using Mangefy.Domain.Owners;
using Mangefy.Domain.Owners.Repositories;
using Mangefy.Domain.Platform.BusinessTypes.Repositories;
using Mangefy.Domain.Plans;
using Mangefy.Domain.Plans.Repositories;
using Mangefy.Domain.Roles;
using Mangefy.Domain.Roles.Repositories;
using Mangefy.Domain.Settings;
using Mangefy.Domain.Settings.Repositories;
using Mangefy.Domain.Stock.Repositories;
using Mangefy.Domain.Tenants;
using Mangefy.Domain.Tenants.Repositories;
using MediatR;

namespace Mangefy.Application.Tenants.Commands.CreateTenant;

public sealed class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Guid>
{
    private readonly ITenantRepository _tenants;
    private readonly IOwnerRepository _owners;
    private readonly IPlanRepository _plans;
    private readonly IBusinessTypeRepository _businessTypes;
    private readonly ITenantRoleRepository _roles;
    private readonly IMenuRepository _menus;
    private readonly IBusinessScheduleRepository _businessSchedules;
    private readonly IPaymentSettingsRepository _paymentSettings;
    private readonly IFiscalSettingsRepository _fiscalSettings;
    private readonly IPrinterSettingsRepository _printerSettings;
    private readonly ITabSettingsRepository _tabSettings;
    private readonly IIntegrationSettingsRepository _integrationSettings;
    private readonly IReservationSettingsRepository _reservationSettings;
    private readonly IStockRepository _stocks;
    private readonly IWorkforceSettingsRepository _workforceSettings;
    private readonly IUnitOfWork _uow;

    public CreateTenantCommandHandler(
        ITenantRepository tenants,
        IOwnerRepository owners,
        IPlanRepository plans,
        IBusinessTypeRepository businessTypes,
        ITenantRoleRepository roles,
        IMenuRepository menus,
        IBusinessScheduleRepository businessSchedules,
        IPaymentSettingsRepository paymentSettings,
        IFiscalSettingsRepository fiscalSettings,
        IPrinterSettingsRepository printerSettings,
        ITabSettingsRepository tabSettings,
        IIntegrationSettingsRepository integrationSettings,
        IReservationSettingsRepository reservationSettings,
        IStockRepository stocks,
        IWorkforceSettingsRepository workforceSettings,
        IUnitOfWork uow)
    {
        _tenants = tenants;
        _owners = owners;
        _plans = plans;
        _businessTypes = businessTypes;
        _roles = roles;
        _menus = menus;
        _businessSchedules = businessSchedules;
        _paymentSettings = paymentSettings;
        _fiscalSettings = fiscalSettings;
        _printerSettings = printerSettings;
        _tabSettings = tabSettings;
        _integrationSettings = integrationSettings;
        _reservationSettings = reservationSettings;
        _stocks = stocks;
        _workforceSettings = workforceSettings;
        _uow = uow;
    }

    public async Task<Guid> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        if (await _tenants.ExistsBySlugAsync(request.Slug, cancellationToken))
            throw new ConflictException($"Slug '{request.Slug}' já está em uso.");

        var owner = await _owners.GetByIdAsync(request.OwnerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Owner), request.OwnerId);

        if (owner.Status == OwnerStatus.Inactive)
            throw new DomainException("Não é possível criar um estabelecimento para um cliente inativo.");

        var plan = await _plans.GetByIdAsync(request.PlanId, cancellationToken)
            ?? throw new NotFoundException(nameof(Plan), request.PlanId);

        if (plan.Status != PlanStatus.Active)
            throw new DomainException("Não é possível criar um estabelecimento com um plano inativo.");

        var businessType = await _businessTypes.GetByIdAsync(request.BusinessTypeId, cancellationToken)
            ?? throw new NotFoundException("BusinessType", request.BusinessTypeId);

        if (!businessType.IsActive)
            throw new DomainException("Não é possível criar um estabelecimento com um tipo de negócio inativo.");

        // 1. Criar Tenant
        var tenant = Tenant.Create(
            request.OwnerId, request.Name, request.Slug,
            request.PlanId, request.BusinessTypeId,
            request.Timezone, request.TrialDays, request.Email);

        await _tenants.AddAsync(tenant, cancellationToken);

        // 1.1 Copiar RoleTemplates ativos do BusinessType para o novo Tenant
        foreach (var template in businessType.GetActiveTemplates())
        {
            var role = TenantRole.CreateFromTemplate(
                tenant.Id, template.Name, template.Description, template.Permissions, template.Id);
            await _roles.AddAsync(role, cancellationToken);
        }

        // 2. Criar configurações padrão do tenant
        var defaultMenu = Menu.CreateDefault(tenant.Id);
        await _menus.AddAsync(defaultMenu, cancellationToken);

        var businessSchedule = BusinessSchedule.Create(tenant.Id);
        await _businessSchedules.AddAsync(businessSchedule, cancellationToken);

        var paySettings = PaymentSettings.CreateDefault(tenant.Id);
        await _paymentSettings.AddAsync(paySettings, cancellationToken);

        var fiscSettings = FiscalSettings.CreateDefault(tenant.Id);
        await _fiscalSettings.AddAsync(fiscSettings, cancellationToken);

        var prnSettings = PrinterSettings.Create(tenant.Id);
        await _printerSettings.AddAsync(prnSettings, cancellationToken);

        var tabSett = Domain.Settings.TabSettings.CreateDefault(tenant.Id);
        await _tabSettings.AddAsync(tabSett, cancellationToken);

        var intSett = IntegrationSettings.CreateDefault(tenant.Id);
        await _integrationSettings.AddAsync(intSett, cancellationToken);

        var resSett = ReservationSettings.CreateDefault(tenant.Id);
        await _reservationSettings.AddAsync(resSett, cancellationToken);

        var stock = Mangefy.Domain.Stock.Stock.Create(tenant.Id);
        await _stocks.AddAsync(stock, cancellationToken);

        var workforceSett = Domain.Settings.WorkforceSettings.CreateDefault(tenant.Id);
        await _workforceSettings.AddAsync(workforceSett, cancellationToken);

        await _uow.SaveChangesAsync(cancellationToken);

        return tenant.Id;
    }
}
