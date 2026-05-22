using Mangefy.Domain.Audit;
using Mangefy.Domain.Owners;
using Mangefy.Domain.BusinessSchedules;
using Mangefy.Domain.Devices;
using Mangefy.Domain.Idempotency;
using Mangefy.Domain.OperationalSessions;
using Mangefy.Domain.PrintJobs;
using Mangefy.Domain.DailyCash;
using Mangefy.Domain.EmployeeSchedules;
using Mangefy.Domain.Employees;
using Mangefy.Domain.Fiscal;
using Mangefy.Domain.Menus;
using Mangefy.Domain.Platform.BusinessTypes;
using Mangefy.Domain.Platform.Features;
using Mangefy.Domain.Platform.SupplierCategories;
using Mangefy.Domain.Platform.Suppliers;
using Mangefy.Domain.Platform.Subscriptions;
using Mangefy.Domain.Plans;
using Mangefy.Domain.Reservations;
using Mangefy.Domain.Roles;
using Mangefy.Domain.Settings;
using Mangefy.Domain.Stock;
using Mangefy.Domain.Tables;
using Mangefy.Domain.Tabs;
using Mangefy.Domain.Tenants;
using Microsoft.EntityFrameworkCore;

namespace Mangefy.Infrastructure.Persistence;

public class MangefyDbContext : DbContext
{
    public MangefyDbContext(DbContextOptions<MangefyDbContext> options) : base(options) { }

    // Platform
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<BusinessType> BusinessTypes => Set<BusinessType>();
    public DbSet<PlanFeatureSet> PlanFeatureSets => Set<PlanFeatureSet>();
    public DbSet<FeatureGracePeriod> FeatureGracePeriods => Set<FeatureGracePeriod>();
    public DbSet<SupplierCategory> SupplierCategories => Set<SupplierCategory>();
    public DbSet<PlatformSupplier> PlatformSuppliers => Set<PlatformSupplier>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    // Owners (plataforma)
    public DbSet<Owner> Owners => Set<Owner>();
    public DbSet<OwnerActivationToken> OwnerActivationTokens => Set<OwnerActivationToken>();

    // Tenant
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantRole> TenantRoles => Set<TenantRole>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<ActivationToken> ActivationTokens => Set<ActivationToken>();
    public DbSet<EmployeeSchedule> EmployeeSchedules => Set<EmployeeSchedule>();
    public DbSet<BusinessSchedule> BusinessSchedules => Set<BusinessSchedule>();

    // Settings
    public DbSet<PaymentSettings> PaymentSettings => Set<PaymentSettings>();
    public DbSet<FiscalSettings> FiscalSettings => Set<FiscalSettings>();
    public DbSet<PrinterSettings> PrinterSettings => Set<PrinterSettings>();
    public DbSet<IntegrationSettings> IntegrationSettings => Set<IntegrationSettings>();
    public DbSet<TabSettings> TabSettings => Set<TabSettings>();
    public DbSet<WorkforceSettings> WorkforceSettings => Set<WorkforceSettings>();
    public DbSet<ReservationSettings> ReservationSettings => Set<ReservationSettings>();

    // Menu
    public DbSet<Menu> Menus => Set<Menu>();

    // Audit & Fiscal
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<FiscalDocument> FiscalDocuments => Set<FiscalDocument>();

    // Idempotency
    public DbSet<IdempotencyEntry> IdempotencyEntries => Set<IdempotencyEntry>();

    // Devices & Sessions
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<OperationalSession> OperationalSessions => Set<OperationalSession>();

    // Print Jobs
    public DbSet<PrintJob> PrintJobs => Set<PrintJob>();

    // Operations
    public DbSet<Table> Tables => Set<Table>();
    public DbSet<Tab> Tabs => Set<Tab>();
    public DbSet<Stock> Stocks => Set<Stock>();
    public DbSet<Domain.Stock.Supplier> Suppliers => Set<Domain.Stock.Supplier>();
    public DbSet<CashRegister> CashRegisters => Set<CashRegister>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<Mangefy.Domain.Common.DomainEvent>();
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MangefyDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
