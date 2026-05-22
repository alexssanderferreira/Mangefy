# Camada de Infraestrutura — Mangefy

Documentação da camada Infrastructure: EF Core, repositórios, serviços de autenticação.

---

## Visão Geral

A camada Infrastructure implementa as interfaces definidas na Application. Contém o DbContext, as configurações EF Core, os repositórios e os serviços de autenticação (JWT e BCrypt).

**Projeto:** `Mangefy.Infrastructure`  
**Pacotes principais:**
- Npgsql.EntityFrameworkCore.PostgreSQL 8.0.11 (provider PostgreSQL)
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
│   │   ├── EmployeeConfiguration.cs          ← + RowVersion
│   │   ├── EmployeeScheduleConfiguration.cs
│   │   ├── MenuConfiguration.cs              ← + PromotionalPrice (OwnsOne), PromotionValidUntil, PriceHistory (OwnsMany → MenuItemPriceHistory)
│   │   ├── TableConfiguration.cs
│   │   ├── TabConfiguration.cs               ← + RowVersion
│   │   ├── StockConfiguration.cs             ← + RowVersion
│   │   ├── SupplierConfiguration.cs
│   │   ├── CashRegisterConfiguration.cs      ← + RowVersion, CashSupplies (tabela), CashMethodBalances (tabela com shadow int PK)
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
│   │   ├── TabSettingsConfiguration.cs       ← + MaxDiscountPercent, DiscountReasonRequiredAbove
│   │   ├── WorkforceSettingsConfiguration.cs
│   │   ├── ReservationSettingsConfiguration.cs
│   │   ├── IdempotencyEntryConfiguration.cs  ← tabela IdempotencyEntries; índice único (TenantId, CommandId)
│   │   ├── DeviceConfiguration.cs            ← tabela Devices; índice único (TenantId, PublicIdentifier)
│   │   ├── OperationalSessionConfiguration.cs ← tabela OperationalSessions
│   │   └── PrintJobConfiguration.cs          ← tabela PrintJobs
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
│       ├── ReservationSettingsRepository.cs
│       ├── IdempotencyRepository.cs
│       ├── DeviceRepository.cs
│       ├── OperationalSessionRepository.cs
│       ├── PrintJobRepository.cs
│       ├── PlatformSupplierRepository.cs    ← implementa IPlatformSupplierRepository
│       └── SupplierCategoryRepository.cs   ← implementa ISupplierCategoryRepository
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
| Tenant | Tenants, TenantRoles, Employees, **ActivationTokens**, EmployeeSchedules, BusinessSchedules |
| Settings | PaymentSettings, FiscalSettings, PrinterSettings, IntegrationSettings, TabSettings, WorkforceSettings, ReservationSettings |
| Operações | Menus, Tables, Tabs, Stocks, Suppliers, CashRegisters, Reservations |
| Auditoria & Fiscal | **AuditLogs**, **FiscalDocuments** |
| Robustez | **IdempotencyEntries**, **Devices**, **OperationalSessions**, **PrintJobs** |

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

## Concorrência Otimista

`Tab`, `CashRegister`, `Stock` e `Employee` têm `byte[] RowVersion` mapeado com `.IsRowVersion()`. O `UnitOfWork` captura `DbUpdateConcurrencyException` e relança como `ConflictException` (HTTP 409) com mensagem amigável identificando o nome da entidade.

---

## UnitOfWork e Domain Events

`UnitOfWork` implementa `IUnitOfWork`. Após `SaveChangesAsync`, coleta todos os `AggregateRoot` rastreados pelo EF Core com eventos pendentes e os publica via `IPublisher` (MediatR):

```csharp
var result = await _context.SaveChangesAsync(ct);
var aggregates = _context.ChangeTracker
    .Entries<AggregateRoot>()
    .Where(e => e.Entity.DomainEvents.Any())
    .Select(e => e.Entity)
    .ToList();
foreach (var aggregate in aggregates)
{
    var events = aggregate.DomainEvents.ToList();
    aggregate.ClearDomainEvents();
    foreach (var domainEvent in events)
        await _publisher.Publish(domainEvent, ct);
}
// Segundo save: persiste mudanças feitas pelos event handlers (ex.: Table.Occupy())
if (_context.ChangeTracker.HasChanges())
    await _context.SaveChangesAsync(ct);
```

`DomainEvent` implementa `INotification` (de `MediatR.Contracts`, referenciado no projeto Domain). Handlers de eventos são registrados via `INotificationHandler<TEvent>` na camada Application ou Infrastructure.

> **Rastreabilidade automática:** Antes de `SaveChangesAsync`, `UnitOfWork.ApplyAuditInfo()` itera sobre todas as entradas `Added` e `Modified` do ChangeTracker. Se `ICurrentUser.EmployeeId` não for nulo, chama `Entity.SetCreatedByEmployee(id)` ou `SetUpdatedByEmployee(id)` respectivamente. AdminSaas não tem `EmployeeId` — os campos ficam nulos para entidades de plataforma.

**Handlers de domain events registrados:**
| Evento | Handler | Localização |
|--------|---------|-------------|
| `OrderReadyEvent` | `OrderReadyEventHandler` | Application/Tabs/EventHandlers |
| `TabOpenedEvent` | `TabOpenedEventHandler` | Application/Tabs/EventHandlers |
| `TabClosedEvent` | `TabClosedEventHandler` | Application/Tabs/EventHandlers |
| `TabCancelledEvent` | `TabCancelledEventHandler` | Application/Tabs/EventHandlers |
| `TenantPlanChangedEvent` | `TenantPlanChangedEventHandler` | Application/Tenants/EventHandlers |
| `ReservationArrivedEvent` | `ReservationArrivedEventHandler` (informativo — reservado para notificações futuras) | Application/Reservations/EventHandlers |

**Pendentes (futuro):** `StockLowEvent` (alerta push), `InvoiceOverdueEvent` (automação de cobrança).

> **Nota — fluxo de chegada de reserva:** A abertura automática da Tab ocorre em `RegisterArrivalCommandHandler` (não no event handler). O handler valida o status da reserva, chama `ISender.Send(OpenTabCommand)` com `CustomerName` e `TableId` da reserva (fallback `LocationNote = "Reserva"` quando `TableId` for nulo), e só então chama `Reservation.RegisterArrival(tabId)`. O `ReservationArrivedEventHandler` é informativo e reservado para notificações futuras.

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
- `IPlanRepository`: `GetAllActiveAsync` (só ativos), `GetAllAsync` (todos, para admin), `DeleteAsync`
- `IBusinessTypeRepository`: `GetByIdAsync` (com `Include(x => x.RoleTemplates)`), `GetAllActiveAsync`, `GetAllAsync`, `ExistsByNameAsync`, `AddAsync`, `UpdateAsync` (ver nota abaixo), `DeleteAsync`
- `ITenantRepository`: CRUD + `CountByBusinessTypeAsync()` → `Dictionary<Guid, int>` (GROUP BY BusinessTypeId)
- `ITenantRoleRepository`: CRUD + `GetByTenantAsync`, `GetOwnerRoleByTenantAsync`, `ExistsByNameAsync`, `CountByTemplateIdAsync()` → `Dictionary<Guid, int>` (GROUP BY TemplateId WHERE TemplateId IS NOT NULL)
- `IEmployeeRepository`: `GetByEmailAsync` (global — mantido para uso futuro), `GetByEmailInTenantAsync(tenantId, email)` (por tenant — **usado no login**), `ExistsByEmailInTenantAsync(tenantId, email)` (por tenant — usado na criação), `GetByIdAsync`, `GetByTenantAsync`, `GetByRoleAsync(tenantRoleId)`
- `IMenuRepository`: `GetByIdAsync`, `GetAllByTenantAsync`
- `ITableRepository`: `GetByNumberAsync`
- `ITabRepository`: `GetOpenByTenantAsync`, `GetByIdAsync`, **`GetClosedByPeriodAsync`**
- `ICashRegisterRepository`: `GetOpenByTenantAsync`
- `IReservationRepository`: `GetByDateAsync`
- `IStockRepository`: `GetByTenantIdAsync`
- `IAuditLogRepository`: `AddAsync`, `GetByTenantAsync(from, to)`
- `IFiscalDocumentRepository`: `GetByIdAsync`, `GetByTabIdAsync`, `GetByTenantAsync(from, to)`, `AddAsync`, `UpdateAsync`

> **`BusinessTypeRepository.UpdateAsync` — armadilha EF Core OwnsMany:**
> Chamar `context.Update(owner)` recursivamente marca owned entities como `Modified`, gerando `UPDATE` em vez de `INSERT` para novos templates.
> Chamar `Entry(entity).State` dispara `DetectChanges` internamente, corrompendo o estado dos novos owned entities antes de qualquer verificação.
> **Solução:** Desabilitar `AutoDetectChangesEnabled = false` no bloco try, verificar e setar estados manualmente, reabilitar no finally. Novos templates (Detached) → `Added`. Templates removidos da collection → `Deleted` (detectados por comparação com os tracked entries do DbContext).

---

## Registro de Dependências

```csharp
// Auth
services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
services.AddScoped<ITokenService, JwtTokenService>();
services.AddSingleton<IAdminSaasCredentials, AdminSaasCredentials>();

// UoW
services.AddScoped<IUnitOfWork, UnitOfWork>();

// Application services
services.AddScoped<IAuditService, AuditService>();
services.AddScoped<IFeatureGateService, FeatureGateService>();

// Repositórios (operacionais)
services.AddScoped<ITenantRepository, TenantRepository>();
services.AddScoped<IPlanRepository, PlanRepository>();
services.AddScoped<IBusinessTypeRepository, BusinessTypeRepository>();
services.AddScoped<ITenantRoleRepository, TenantRoleRepository>();
services.AddScoped<IEmployeeRepository, EmployeeRepository>();
services.AddScoped<IActivationTokenRepository, ActivationTokenRepository>();
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

// Feature gate
services.AddScoped<IPlanFeatureSetRepository, PlanFeatureSetRepository>();
services.AddScoped<IFeatureGracePeriodRepository, FeatureGracePeriodRepository>();

// Auditoria & Fiscal
services.AddScoped<IAuditLogRepository, AuditLogRepository>();
services.AddScoped<IFiscalDocumentRepository, FiscalDocumentRepository>();

// Robustez
services.AddScoped<IIdempotencyRepository, IdempotencyRepository>();
services.AddScoped<IDeviceRepository, DeviceRepository>();
services.AddScoped<IOperationalSessionRepository, OperationalSessionRepository>();
services.AddScoped<IPrintJobRepository, PrintJobRepository>();
services.AddScoped<IShiftEnforcementService, ShiftEnforcementService>();
```

---

## Serviços de Aplicação (Infrastructure)

### AuditService
Implementa `IAuditService`. Delega para `IAuditLogRepository.AddAsync` — um passo de indireção para permitir enriquecer o log (ex.: contexto da requisição) sem poluir os handlers.

### FeatureGateService
Implementa `IFeatureGateService`. Fluxo de `IsEnabledAsync(tenantId, featureKey)`:
1. Carrega `Tenant` → obtém `PlanId` e `BusinessTypeId`
2. Busca `PlanFeatureSet` pela combinação → verifica `HasFeature(featureKey)`
3. Se não encontrado no plano, busca `FeatureGracePeriod` ativo pelo tenant + featureKey
4. Retorna `true` se feature consta no plano **ou** carência ainda válida

### ShiftEnforcementService
Implementa `IShiftEnforcementService`. Fluxo de `CanOperateAsync(tenantId, employeeId)`:
1. Owner (`IsOwner = true`) → `true` imediatamente
2. Acesso temporário válido (`HasTemporaryAccess()`) → `true`
3. Sem schedule cadastrado → `true` (sem restrição)
4. Verifica `EmployeeSchedule.IsOnDutyAt(DateTime.UtcNow)` incluindo tolerância de `WorkforceSettings.ShiftToleranceMinutes`

---

## Configurações EF Core — Novos Agregados

### TabConfiguration (atualizado)
Adicionados ao mapeamento existente:
- `Tab`: `Channel` (string, 30), `DiscountAmount`, `ServiceFee`, `Tip` (decimal 10,2), `RowVersion`
- `Tab.DeliveryInfo`: `OwnsOne` com colunas `DeliveryRecipientName`, `DeliveryAddress`, `DeliveryComplement`, `DeliveryPhone`, `DeliveryExternalRef`
- `TabPayments`: `ChangeGiven` (decimal), `ExternalReference` (string 200)
- `OrderItems`: `Station` (string 30), `Priority`, `CancellationReason` (500), `DiscountAmount`, `PreparationStartedAt`, `DeliveredAt`, `IsReturned`; `_modifiers` como `List<string>` serializada com separador `|` + `ValueComparer`

### AuditLogConfiguration
Tabela `AuditLogs`. Índices em `(TenantId, OccurredAt)` e `(EntityType, EntityId)`.

### FiscalDocumentConfiguration
Tabela `FiscalDocuments`. `TotalAmount` como `OwnsOne`. Índices em `(TenantId, CreatedAt)` e `TabId`.

### CashRegisterConfiguration (atualizado)
- `RowVersion`
- `CashSupplies`: `OwnsMany` → tabela `CashSupplies` com `OwnsOne` de `Amount` (Money)
- `CashMethodBalances`: `OwnsMany` → tabela `CashMethodBalances` com shadow int PK (`mb.Property<int>("Id").ValueGeneratedOnAdd(); mb.HasKey("Id")`)

### MenuConfiguration (atualizado)
- `MenuItem.PromotionalPrice`: `OwnsOne` com colunas `PromotionalPriceAmount`, `PromotionalPriceCurrency`
- `MenuItem.PromotionValidUntil`: coluna datetime nullable
- `MenuItem.PriceHistory`: `OwnsMany` → tabela `MenuItemPriceHistory`

### IdempotencyEntryConfiguration
Tabela `IdempotencyEntries`. Índice único em `(TenantId, CommandId)`.

### DeviceConfiguration
Tabela `Devices`. Índice único em `(TenantId, PublicIdentifier)`.

### OperationalSessionConfiguration
Tabela `OperationalSessions`.

### PrintJobConfiguration
Tabela `PrintJobs`.

---

## Migrations

```bash
# Gerar nova migration após mudanças no modelo (PostgreSQL via Npgsql):
cd Backend
dotnet ef migrations add <NomeDaMigration> --project Mangefy.Infrastructure --startup-project Mangefy.API
dotnet ef database update --project Mangefy.Infrastructure --startup-project Mangefy.API
# Requer PostgreSQL rodando e string de conexão configurada em appsettings.Development.json ou user-secrets
```

Migration gerada (schema completo PostgreSQL via Npgsql):
- `InitialPostgres` — schema completo: todas as tabelas, índices, RowVersion como `bytea`, campos novos em `Tabs`/`TabPayments`/`OrderItems`/`MenuItems`/`TabSettings`; índice filtrado `{TenantId, Number} WHERE "Status" = 'Open'` em `Tabs`; índice único `{TenantId, Email}` em `Employees` via SQL puro *(gerada; pendente de aplicar ao banco)*

> Para aplicar: requer instância PostgreSQL rodando. Configurar credenciais via user secrets (`dotnet user-secrets set "ConnectionStrings:DefaultConnection" "..."`) e rodar `dotnet ef database update`.

A connection string padrão em `appsettings.Development.json` aponta para PostgreSQL local:
```
Host=localhost;Port=5432;Database=MangefyDb;Username=postgres;Password=SET_VIA_USER_SECRETS
```
