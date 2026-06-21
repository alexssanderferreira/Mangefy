using FluentAssertions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Application.Tenants.Commands.CreateTenant;
using Mangefy.Domain.BusinessSchedules;
using Mangefy.Domain.BusinessSchedules.Repositories;
using Mangefy.Domain.Menus;
using Mangefy.Domain.Menus.Repositories;
using Mangefy.Domain.Owners;
using Mangefy.Domain.Owners.Repositories;
using Mangefy.Domain.Platform.BusinessTypes;
using Mangefy.Domain.Platform.BusinessTypes.Repositories;
using Mangefy.Domain.Plans;
using Mangefy.Domain.Plans.Repositories;
using Mangefy.Domain.Roles;
using Mangefy.Domain.Roles.Repositories;
using Mangefy.Domain.Settings;
using Mangefy.Domain.Settings.Repositories;
using DomainWorkforceSettings = Mangefy.Domain.Settings.WorkforceSettings;
using Mangefy.Domain.Stock.Repositories;
using Mangefy.Domain.Tenants.Repositories;
using Moq;

namespace Mangefy.Application.Tests.Tenants.Commands;

public sealed class CreateTenantCommandHandlerTests
{
    private readonly Mock<ITenantRepository> _tenants = new();
    private readonly Mock<IOwnerRepository> _owners = new();
    private readonly Mock<IPlanRepository> _plans = new();
    private readonly Mock<IBusinessTypeRepository> _businessTypes = new();
    private readonly Mock<ITenantRoleRepository> _roles = new();
    private readonly Mock<IMenuRepository> _menus = new();
    private readonly Mock<IBusinessScheduleRepository> _businessSchedules = new();
    private readonly Mock<IPaymentSettingsRepository> _paymentSettings = new();
    private readonly Mock<IFiscalSettingsRepository> _fiscalSettings = new();
    private readonly Mock<IPrinterSettingsRepository> _printerSettings = new();
    private readonly Mock<ITabSettingsRepository> _tabSettings = new();
    private readonly Mock<IIntegrationSettingsRepository> _integrationSettings = new();
    private readonly Mock<IReservationSettingsRepository> _reservationSettings = new();
    private readonly Mock<IStockRepository> _stocks = new();
    private readonly Mock<IWorkforceSettingsRepository> _workforceSettings = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    private readonly CreateTenantCommandHandler _handler;

    public CreateTenantCommandHandlerTests()
    {
        _handler = new CreateTenantCommandHandler(
            _tenants.Object, _owners.Object, _plans.Object,
            _businessTypes.Object, _roles.Object, _menus.Object,
            _businessSchedules.Object, _paymentSettings.Object,
            _fiscalSettings.Object, _printerSettings.Object,
            _tabSettings.Object, _integrationSettings.Object,
            _reservationSettings.Object, _stocks.Object,
            _workforceSettings.Object, _uow.Object);

        // Comportamentos padrão que nunca variam entre os testes
        _tenants.Setup(r => r.ExistsBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _tenants.Setup(r => r.AddAsync(It.IsAny<Domain.Tenants.Tenant>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _roles.Setup(r => r.AddAsync(It.IsAny<TenantRole>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _menus.Setup(r => r.AddAsync(It.IsAny<Menu>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _businessSchedules.Setup(r => r.AddAsync(It.IsAny<BusinessSchedule>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _paymentSettings.Setup(r => r.AddAsync(It.IsAny<PaymentSettings>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _fiscalSettings.Setup(r => r.AddAsync(It.IsAny<FiscalSettings>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _printerSettings.Setup(r => r.AddAsync(It.IsAny<PrinterSettings>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _tabSettings.Setup(r => r.AddAsync(It.IsAny<TabSettings>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _integrationSettings.Setup(r => r.AddAsync(It.IsAny<IntegrationSettings>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _reservationSettings.Setup(r => r.AddAsync(It.IsAny<ReservationSettings>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _stocks.Setup(r => r.AddAsync(It.IsAny<Domain.Stock.Stock>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _workforceSettings.Setup(r => r.AddAsync(It.IsAny<DomainWorkforceSettings>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    // ── Testes de criação de cargos ──────────────────────────────────────────

    [Fact]
    public async Task Handle_SemTemplates_DeveCriarApenasOwnerRole()
    {
        // Arrange
        var businessType = BusinessType.Create("Barzinho");
        // sem templates adicionados

        var (owner, plan, command) = CriarCenarioPadrao(businessType.Id);
        _businessTypes.Setup(r => r.GetByIdAsync(businessType.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(businessType);
        _owners.Setup(r => r.GetByIdAsync(owner.Id, It.IsAny<CancellationToken>())).ReturnsAsync(owner);
        _plans.Setup(r => r.GetByIdAsync(plan.Id, It.IsAny<CancellationToken>())).ReturnsAsync(plan);

        var rolesAdicionados = CapturarRolesAdicionados();

        // Act
        await _handler.Handle(command with { OwnerId = owner.Id, PlanId = plan.Id, BusinessTypeId = businessType.Id }, default);

        // Assert
        rolesAdicionados.Should().HaveCount(1);
        rolesAdicionados[0].IsOwnerRole.Should().BeTrue();
        rolesAdicionados[0].Name.Should().Be("Dono");
        rolesAdicionados[0].IsFromTemplate.Should().BeFalse();
        rolesAdicionados[0].IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task Handle_ComNTemplates_DeveCriarOwnerRoleMaisNRolesDeTemplate(int quantidadeTemplates)
    {
        // Arrange
        var businessType = BusinessType.Create("Restaurante");
        for (var i = 1; i <= quantidadeTemplates; i++)
            businessType.AddRoleTemplate($"Cargo {i}", $"Descrição do cargo {i}");

        var (owner, plan, command) = CriarCenarioPadrao(businessType.Id);
        _businessTypes.Setup(r => r.GetByIdAsync(businessType.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(businessType);
        _owners.Setup(r => r.GetByIdAsync(owner.Id, It.IsAny<CancellationToken>())).ReturnsAsync(owner);
        _plans.Setup(r => r.GetByIdAsync(plan.Id, It.IsAny<CancellationToken>())).ReturnsAsync(plan);

        var rolesAdicionados = CapturarRolesAdicionados();

        // Act
        await _handler.Handle(command with { OwnerId = owner.Id, PlanId = plan.Id, BusinessTypeId = businessType.Id }, default);

        // Assert: 1 OwnerRole + N roles de template
        rolesAdicionados.Should().HaveCount(quantidadeTemplates + 1);

        var ownerRole = rolesAdicionados.Single(r => r.IsOwnerRole);
        ownerRole.Name.Should().Be("Dono");
        ownerRole.IsFromTemplate.Should().BeFalse();

        var templateRoles = rolesAdicionados.Where(r => !r.IsOwnerRole).ToList();
        templateRoles.Should().HaveCount(quantidadeTemplates);
        templateRoles.Should().AllSatisfy(r =>
        {
            r.IsFromTemplate.Should().BeTrue();
            r.TemplateId.Should().NotBeNull();
            r.IsActive.Should().BeTrue();
            r.TenantId.Should().NotBeEmpty();
        });
    }

    [Fact]
    public async Task Handle_ComTemplatesAtivosEInativos_DeveCriarRolesApenasParaAtivos()
    {
        // Arrange
        var businessType = BusinessType.Create("Padaria");
        businessType.AddRoleTemplate("Atendente");
        var templateInativo = businessType.AddRoleTemplate("Caixa");
        businessType.DeactivateRoleTemplate(templateInativo.Id);

        var (owner, plan, command) = CriarCenarioPadrao(businessType.Id);
        _businessTypes.Setup(r => r.GetByIdAsync(businessType.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(businessType);
        _owners.Setup(r => r.GetByIdAsync(owner.Id, It.IsAny<CancellationToken>())).ReturnsAsync(owner);
        _plans.Setup(r => r.GetByIdAsync(plan.Id, It.IsAny<CancellationToken>())).ReturnsAsync(plan);

        var rolesAdicionados = CapturarRolesAdicionados();

        // Act
        await _handler.Handle(command with { OwnerId = owner.Id, PlanId = plan.Id, BusinessTypeId = businessType.Id }, default);

        // Assert: OwnerRole + apenas 1 template ativo (Caixa foi ignorado)
        rolesAdicionados.Should().HaveCount(2);
        rolesAdicionados.Count(r => r.IsOwnerRole).Should().Be(1);
        rolesAdicionados.Count(r => r.IsFromTemplate).Should().Be(1);
        rolesAdicionados.Single(r => r.IsFromTemplate).Name.Should().Be("Atendente");
    }

    [Fact]
    public async Task Handle_OwnerRole_DeveTerTenantIdCorreto()
    {
        // Arrange
        var businessType = BusinessType.Create("Cafeteria");
        var (owner, plan, command) = CriarCenarioPadrao(businessType.Id);
        _businessTypes.Setup(r => r.GetByIdAsync(businessType.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(businessType);
        _owners.Setup(r => r.GetByIdAsync(owner.Id, It.IsAny<CancellationToken>())).ReturnsAsync(owner);
        _plans.Setup(r => r.GetByIdAsync(plan.Id, It.IsAny<CancellationToken>())).ReturnsAsync(plan);

        Guid? tenantIdCapturado = null;
        _tenants.Setup(r => r.AddAsync(It.IsAny<Domain.Tenants.Tenant>(), It.IsAny<CancellationToken>()))
            .Callback<Domain.Tenants.Tenant, CancellationToken>((t, _) => tenantIdCapturado = t.Id)
            .Returns(Task.CompletedTask);

        var rolesAdicionados = CapturarRolesAdicionados();

        // Act
        await _handler.Handle(command with { OwnerId = owner.Id, PlanId = plan.Id, BusinessTypeId = businessType.Id }, default);

        // Assert: o TenantId do OwnerRole deve ser o mesmo do Tenant criado
        tenantIdCapturado.Should().NotBeNull();
        rolesAdicionados.Should().AllSatisfy(r => r.TenantId.Should().Be(tenantIdCapturado!.Value));
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static (Owner owner, Plan plan, CreateTenantCommand command) CriarCenarioPadrao(Guid businessTypeId)
    {
        var owner = Owner.Create("João Silva", "joao@example.com");
        owner.Activate();

        var plan = Plan.Create("Starter", 99m, maxTables: 10, maxMenuItems: 50, maxUsers: 5);

        var command = new CreateTenantCommand(
            OwnerId: owner.Id,
            Name: "Restaurante do João",
            Slug: "restaurante-do-joao",
            PlanId: plan.Id,
            BusinessTypeId: businessTypeId);

        return (owner, plan, command);
    }

    private List<TenantRole> CapturarRolesAdicionados()
    {
        var lista = new List<TenantRole>();
        _roles.Setup(r => r.AddAsync(It.IsAny<TenantRole>(), It.IsAny<CancellationToken>()))
            .Callback<TenantRole, CancellationToken>((role, _) => lista.Add(role))
            .Returns(Task.CompletedTask);
        return lista;
    }
}
