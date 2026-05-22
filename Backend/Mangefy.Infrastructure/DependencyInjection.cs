using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Owners.Repositories;
using Mangefy.Domain.Audit.Repositories;
using Mangefy.Domain.Devices.Repositories;
using Mangefy.Domain.Fiscal.Repositories;
using Mangefy.Domain.Idempotency.Repositories;
using Mangefy.Domain.OperationalSessions.Repositories;
using Mangefy.Domain.Platform.Features.Repositories;
using Mangefy.Domain.Platform.Suppliers.Repositories;
using Mangefy.Domain.Platform.SupplierCategories.Repositories;
using Mangefy.Domain.PrintJobs.Repositories;
using Mangefy.Infrastructure.Auth;
using Mangefy.Infrastructure.Services;
using Mangefy.Infrastructure.Services.Email;
using Mangefy.Domain.BusinessSchedules.Repositories;
using Mangefy.Domain.EmployeeSchedules.Repositories;
using Mangefy.Domain.Platform.BusinessTypes.Repositories;
using Mangefy.Domain.Platform.Subscriptions.Repositories;
using Mangefy.Domain.Plans.Repositories;
using Mangefy.Domain.DailyCash.Repositories;
using Mangefy.Domain.Employees.Repositories;
using Mangefy.Domain.Menus.Repositories;
using Mangefy.Domain.Reservations.Repositories;
using Mangefy.Domain.Roles.Repositories;
using Mangefy.Domain.Settings.Repositories;
using Mangefy.Domain.Stock.Repositories;
using Mangefy.Domain.Tables.Repositories;
using Mangefy.Domain.Tabs.Repositories;
using Mangefy.Domain.Tenants.Repositories;
using Mangefy.Infrastructure.Persistence;
using Mangefy.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mangefy.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<MangefyDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsAssembly(typeof(MangefyDbContext).Assembly.FullName)));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Auth services
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddSingleton<IAdminSaasCredentials, AdminSaasCredentials>();

        // Repositories — owners (plataforma)
        services.AddScoped<IOwnerRepository, OwnerRepository>();
        services.AddScoped<IOwnerActivationTokenRepository, OwnerActivationTokenRepository>();

        // Repositories — tenant & employees
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IPlanRepository, PlanRepository>();
        services.AddScoped<IBusinessTypeRepository, BusinessTypeRepository>();
        services.AddScoped<ITenantRoleRepository, TenantRoleRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IActivationTokenRepository, ActivationTokenRepository>();

        // Repositories — operations
        services.AddScoped<IMenuRepository, MenuRepository>();
        services.AddScoped<ITableRepository, TableRepository>();
        services.AddScoped<ITabRepository, TabRepository>();
        services.AddScoped<IStockRepository, StockRepository>();
        services.AddScoped<ICashRegisterRepository, CashRegisterRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();

        // Repositories — schedules
        services.AddScoped<IEmployeeScheduleRepository, EmployeeScheduleRepository>();
        services.AddScoped<IBusinessScheduleRepository, BusinessScheduleRepository>();

        // Repositories — settings
        services.AddScoped<IWorkforceSettingsRepository, WorkforceSettingsRepository>();
        services.AddScoped<IPaymentSettingsRepository, PaymentSettingsRepository>();
        services.AddScoped<IFiscalSettingsRepository, FiscalSettingsRepository>();
        services.AddScoped<IPrinterSettingsRepository, PrinterSettingsRepository>();
        services.AddScoped<ITabSettingsRepository, TabSettingsRepository>();
        services.AddScoped<IIntegrationSettingsRepository, IntegrationSettingsRepository>();
        services.AddScoped<IReservationSettingsRepository, ReservationSettingsRepository>();

        // Repositories — platform AdminSaas
        services.AddScoped<IPlatformSupplierRepository, PlatformSupplierRepository>();
        services.AddScoped<ISupplierCategoryRepository, SupplierCategoryRepository>();

        // Repositories — feature gate
        services.AddScoped<IPlanFeatureSetRepository, PlanFeatureSetRepository>();
        services.AddScoped<IFeatureGracePeriodRepository, FeatureGracePeriodRepository>();

        // Repositories — audit & fiscal
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IFiscalDocumentRepository, FiscalDocumentRepository>();

        // Repositories — idempotency
        services.AddScoped<IIdempotencyRepository, IdempotencyRepository>();

        // Repositories — devices & sessions
        services.AddScoped<IDeviceRepository, DeviceRepository>();
        services.AddScoped<IOperationalSessionRepository, OperationalSessionRepository>();

        // Repositories — print jobs
        services.AddScoped<IPrintJobRepository, PrintJobRepository>();

        // Application services
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IFeatureGateService, FeatureGateService>();
        services.AddScoped<IShiftEnforcementService, ShiftEnforcementService>();

        // Email — Resend
        services.Configure<ResendOptions>(configuration.GetSection(ResendOptions.SectionName));
        services.AddHttpClient<IEmailSender, ResendEmailSender>();

        return services;
    }
}
