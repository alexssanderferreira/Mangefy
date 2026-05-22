# Camada de Infraestrutura — Mangefy

Documentação da camada Infrastructure: EF Core, repositórios, serviços de autenticação.

---

## Visão Geral

A camada Infrastructure implementa as interfaces definidas na Application. Contém o DbContext, as configurações EF Core, os repositórios e os serviços de autenticação (JWT e BCrypt).

**Projeto:** `Mangefy.Infrastructure`  
**Pacotes principais:**
- Microsoft.EntityFrameworkCore.SqlServer 8.0.11
- Microsoft.EntityFrameworkCore.Design 8.0.11 (para migrations)
- Microsoft.AspNetCore.Authentication.JwtBearer 8.0.11
- BCrypt.Net-Next 4.0.3

---

## Estrutura de Pastas

```
Mangefy.Infrastructure/
├── Auth/
│   ├── JwtTokenService.cs               ← implementa ITokenService
│   ├── BcryptPasswordHasher.cs          ← implementa IPasswordHasher (work factor 12)
│   └── AdminSaasCredentials.cs          ← implementa IAdminSaasCredentials (lê AdminSaas:Email e AdminSaas:PasswordHash da config)
├── Persistence/
│   ├── MangefyDbContext.cs              ← DbContext principal
│   ├── UnitOfWork.cs                    ← implementa IUnitOfWork
│   ├── MangefyDbContextFactory.cs       ← IDesignTimeDbContextFactory para migrations
│   ├── Configurations/                  ← uma por aggregate root
│   │   ├── TenantConfiguration.cs
│   │   ├── TenantRoleConfiguration.cs
│   │   ├── EmployeeConfiguration.cs
│   │   ├── EmployeeScheduleConfiguration.cs
│   │   ├── MenuConfiguration.cs
│   │   ├── TableConfiguration.cs
│   │   ├── TabConfiguration.cs
│   │   ├── StockConfiguration.cs
│   │   ├── SupplierConfiguration.cs
│   │   ├── CashRegisterConfiguration.cs
│   │   ├── ReservationConfiguration.cs
│   │   ├── SubscriptionConfiguration.cs
│   │   ├── PlanConfiguration.cs
│   │   ├── PlanFeatureSetConfiguration.cs
│   │   ├── FeatureGracePeriodConfiguration.cs
│   │   ├── BusinessTypeConfiguration.cs
│   │   ├── BusinessScheduleConfiguration.cs
│   │   ├── SupplierCategoryConfiguration.cs
│   │   ├── PlatformSupplierConfiguration.cs
│   │   ├── PaymentSettingsConfiguration.cs
│   │   ├── FiscalSettingsConfiguration.cs
│   │   ├── PrinterSettingsConfiguration.cs
│   │   ├── IntegrationSettingsConfiguration.cs
│   │   ├── TabSettingsConfiguration.cs
│   │   ├── WorkforceSettingsConfiguration.cs
│   │   └── ReservationSettingsConfiguration.cs
│   └── Repositories/
│       ├── TenantRepository.cs
│       ├── PlanRepository.cs
│       ├── BusinessTypeRepository.cs
│       ├── TenantRoleRepository.cs
│       ├── EmployeeRepository.cs
│       ├── MenuRepository.cs
│       ├── TableRepository.cs
│       ├── TabRepository.cs
│       ├── StockRepository.cs
│       ├── CashRegisterRepository.cs
│       ├── ReservationRepository.cs
│       ├── SubscriptionRepository.cs
│       ├── EmployeeScheduleRepository.cs
│       ├── BusinessScheduleRepository.cs
│       ├── WorkforceSettingsRepository.cs
│       ├── PaymentSettingsRepository.cs
│       ├── FiscalSettingsRepository.cs
│       ├── PrinterSettingsRepository.cs
│       ├── TabSettingsRepository.cs
│       ├── IntegrationSettingsRepository.cs
│       └── ReservationSettingsRepository.cs
└── DependencyInjection.cs
```

---

## DbContext

`MangefyDbContext` registra todas as DbSets e usa `ApplyConfigurationsFromAssembly` para carregar as configurações automaticamente:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Ignore<DomainEvent>();
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(MangefyDbContext).Assembly);
    base.OnModelCreating(modelBuilder);
}
```

**DbSets registrados:**

| Grupo | DbSets |
|-------|--------|
| Platform | Plans, BusinessTypes, PlanFeatureSets, FeatureGracePeriods, SupplierCategories, PlatformSuppliers, Subscriptions |
| Tenant | Tenants, TenantRoles, Employees, EmployeeSchedules, BusinessSchedules |
| Settings | PaymentSettings, FiscalSettings, PrinterSettings, IntegrationSettings, TabSettings, WorkforceSettings, ReservationSettings |
| Operações | Menus, Tables, Tabs, Stocks, Suppliers, CashRegisters, Reservations |

`DomainEvent` é ignorado globalmente em `OnModelCreating` para que a coleção `AggregateRoot.DomainEvents` não seja mapeada como navegação.

---

## Design-Time Factory (Migrations)

`MangefyDbContextFactory` implementa `IDesignTimeDbContextFactory<MangefyDbContext>`. Lê a connection string do arquivo `../Mangefy.API/appsettings.json` para que `dotnet ef migrations add` funcione a partir do projeto Infrastructure sem precisar do host.

```bash
# Rodar do diretório Backend/
dotnet ef migrations add <NomeDaMigration> --project Mangefy.Infrastructure --startup-project Mangefy.API
dotnet ef database update --project Mangefy.Infrastructure --startup-project Mangefy.API
```

---

## Padrões das Configurações EF Core

### Value Objects como OwnsOne
```csharp
builder.OwnsOne(x => x.Address, addr => {
    addr.Property(a => a.Cep).HasColumnName("Cep").HasMaxLength(9);
    // ...
});
```

### Coleções Privadas (Private Backing Fields)
Listas encapsuladas em campos privados usam shadow properties com `ValueComparer`:
```csharp
builder.Property<List<string>>("_permissions")
    .HasField("_permissions")
    .HasColumnName("Permissions")
    .HasConversion(
        v => string.Join(',', v),
        v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
    .Metadata.SetValueComparer(new ValueComparer<List<string>>(
        (c1, c2) => c1!.SequenceEqual(c2!),
        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
        c => c.ToList()));
```

Propriedades que requerem `ValueComparer`: `_permissions` (TenantRole, RoleTemplate), `_enabledFeatures` (PlanFeatureSet), `_blockedActions` (ClosingPolicy), `_activeDays` (MenuSchedule), `_enabledMethods` (PaymentSettings).

### OwnsMany Aninhado
O agregado Menu usa hierarquia profunda `Menu → Category → Item → Recipe → RecipeIngredient`:
- Cada nível usa `.ToTable()` separado
- `RecipeIngredient` é ValueObject: sem `HasKey`, sem `Id` próprio

### Money como OwnsOne
```csharp
builder.OwnsOne(x => x.OpeningAmount, m => {
    m.Property(p => p.Amount).HasColumnName("OpeningAmount").HasColumnType("decimal(18,2)");
    m.Property(p => p.Currency).HasColumnName("OpeningCurrency").HasMaxLength(3);
});
```

---

## Autenticação

### JwtTokenService
Implementa `ITokenService`. Lê configurações de `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpirationMinutes` (padrão: 480 min).

**`GenerateToken`** — token de Employee:
- Claims: `sub` (EmployeeId), `email`, `tenantId`, `jti`, `permission` (múltiplas)

**`GenerateAdminSaasToken`** — token de AdminSaas:
- Claims: `sub` = "adminsaas", `email`, `isAdminSaas` = "true", `jti`
- Sem `tenantId` — AdminSaas não pertence a nenhum tenant

### BcryptPasswordHasher
Implementa `IPasswordHasher`. Usa BCrypt.Net-Next com work factor 12.

### AdminSaasCredentials
Implementa `IAdminSaasCredentials`. Lê `AdminSaas:Email` e `AdminSaas:PasswordHash` do `IConfiguration` no construtor. Registrado como **Singleton**.

> **Configuração necessária em appsettings.json:**
> ```json
> "AdminSaas": {
>   "Email": "admin@mangefy.com",
>   "PasswordHash": "$2a$12$..."
> }
> ```
> O hash deve ser gerado via BCrypt work factor 12. Nunca armazenar a senha em texto puro.

---

## Repositórios

Todos os repositórios seguem o mesmo padrão:

```csharp
public sealed class XRepository : IXRepository
{
    private readonly MangefyDbContext _context;
    public XRepository(MangefyDbContext context) => _context = context;

    public Task<X?> GetByTenantIdAsync(Guid tenantId, CancellationToken ct = default)
        => _context.Xs.FirstOrDefaultAsync(x => x.TenantId == tenantId, ct);

    public async Task AddAsync(X entity, CancellationToken ct = default)
        => await _context.Xs.AddAsync(entity, ct);

    public Task UpdateAsync(X entity, CancellationToken ct = default)
    {
        _context.Xs.Update(entity);
        return Task.CompletedTask;
    }
}
```

Repositórios com métodos adicionais:
- `IEmployeeRepository`: `GetByEmailAsync`, `GetByIdAsync`, `GetByTenantAsync`
- `IMenuRepository`: `GetByIdAsync`, `GetAllByTenantAsync`
- `ITableRepository`: `GetByNumberAsync`
- `ITabRepository`: `GetOpenByTenantAsync`, `GetByIdAsync`
- `ICashRegisterRepository`: `GetOpenByTenantAsync`
- `IReservationRepository`: `GetByDateAsync`
- `IStockRepository`: `GetByTenantIdAsync`

---

## Registro de Dependências

```csharp
// Auth
services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
services.AddScoped<ITokenService, JwtTokenService>();
services.AddSingleton<IAdminSaasCredentials, AdminSaasCredentials>();

// UoW
services.AddScoped<IUnitOfWork, UnitOfWork>();

// Repositórios (operacionais)
services.AddScoped<ITenantRepository, TenantRepository>();
services.AddScoped<IPlanRepository, PlanRepository>();
services.AddScoped<IBusinessTypeRepository, BusinessTypeRepository>();
services.AddScoped<ITenantRoleRepository, TenantRoleRepository>();
services.AddScoped<IEmployeeRepository, EmployeeRepository>();
services.AddScoped<IMenuRepository, MenuRepository>();
services.AddScoped<ITableRepository, TableRepository>();
services.AddScoped<ITabRepository, TabRepository>();
services.AddScoped<IStockRepository, StockRepository>();
services.AddScoped<ICashRegisterRepository, CashRegisterRepository>();
services.AddScoped<IReservationRepository, ReservationRepository>();
services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();

// Repositórios (schedules)
services.AddScoped<IEmployeeScheduleRepository, EmployeeScheduleRepository>();
services.AddScoped<IBusinessScheduleRepository, BusinessScheduleRepository>();

// Repositórios (settings)
services.AddScoped<IWorkforceSettingsRepository, WorkforceSettingsRepository>();
services.AddScoped<IPaymentSettingsRepository, PaymentSettingsRepository>();
services.AddScoped<IFiscalSettingsRepository, FiscalSettingsRepository>();
services.AddScoped<IPrinterSettingsRepository, PrinterSettingsRepository>();
services.AddScoped<ITabSettingsRepository, TabSettingsRepository>();
services.AddScoped<IIntegrationSettingsRepository, IntegrationSettingsRepository>();
services.AddScoped<IReservationSettingsRepository, ReservationSettingsRepository>();
```

---

## Migrations

```bash
# Recriar migration após mudanças no domínio:
cd Backend
dotnet ef migrations add <NomeDaMigration> --project Mangefy.Infrastructure --startup-project Mangefy.API
dotnet ef database update --project Mangefy.Infrastructure --startup-project Mangefy.API
```

Migrations aplicadas:
- `Initial` — schema completo inicial
- `AddWorkforceSettings` — tabela `WorkforceSettings`
- `AddReservationSettings` — tabela `ReservationSettings` *(pendente de aplicar)*

A connection string padrão em `appsettings.json` aponta para LocalDB:
```
Server=(localdb)\mssqllocaldb;Database=MangefyDb;Trusted_Connection=True;
```
