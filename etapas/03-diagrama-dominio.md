# Diagrama de Domínio — Mangefy

> Abra em qualquer editor com suporte a Mermaid (VS Code + extensão, GitHub, Notion).

---

## Diagrama ER — Visão Completa

```mermaid
erDiagram

    %% ─── PLATAFORMA (AdminSaas) ─────────────────────────────────

    BusinessType {
        Guid    Id
        string  Name
        string  Description
        bool    IsActive
    }

    RoleTemplate {
        Guid    Id
        Guid    BusinessTypeId
        string  Name
        string  Description
        list    Permissions
        bool    IsActive
    }

    Plan {
        Guid    Id
        string  Name
        Money   MonthlyPrice
        int     MaxTables
        int     MaxMenuItems
        int     MaxUsers
        int     MaxCustomRoles
        enum    Status
    }

    PlanFeatureSet {
        Guid    Id
        Guid    PlanId
        Guid    BusinessTypeId
        list    EnabledFeatures
    }

    FeatureGracePeriod {
        Guid    Id
        Guid    TenantId
        string  FeatureKey
        date    ExpiresAt
        date    NotifiedAt
    }

    BusinessType ||--o{ RoleTemplate    : "define templates"
    Plan ||--o{ PlanFeatureSet          : "tem configurações"
    BusinessType ||--o{ PlanFeatureSet  : "aparece na matriz"

    %% ─── TENANT ─────────────────────────────────────────────────

    Tenant {
        Guid    Id
        string  Name
        string  Slug
        Email   Email
        Phone   Phone
        Guid    PlanId
        Guid    BusinessTypeId
        enum    Status
        date    TrialEndsAt
    }

    TenantRole {
        Guid    Id
        Guid    TenantId
        string  Name
        list    Permissions
        bool    IsOwnerRole
        bool    IsFromTemplate
        Guid    TemplateId
        bool    IsActive
    }

    Employee {
        Guid    Id
        Guid    TenantId
        string  Name
        Email   Email
        string  PasswordHash
        Guid    TenantRoleId
        bool    IsOwner
        enum    Status
    }

    Plan ||--o{ Tenant              : "contratado por"
    BusinessType ||--o{ Tenant      : "tipo de"
    Tenant ||--o{ TenantRole        : "possui cargos"
    Tenant ||--o{ Employee          : "possui funcionários"
    Tenant ||--o{ FeatureGracePeriod: "tem carências"
    TenantRole ||--o{ Employee      : "atribuído a"
    RoleTemplate ..o{ TenantRole    : "originou (snapshot)"

    %% ─── CARDÁPIO ───────────────────────────────────────────────

    Menu {
        Guid    Id
        Guid    TenantId
        string  Name
        bool    IsActive
    }

    MenuCategory {
        Guid    Id
        Guid    TenantId
        string  Name
        int     DisplayOrder
        bool    IsActive
    }

    MenuItem {
        Guid    Id
        Guid    CategoryId
        string  Name
        Money   Price
        bool    RequiresKds
        enum    Status
    }

    Tenant ||--|| Menu           : "tem um"
    Menu ||--o{ MenuCategory     : "contém"
    MenuCategory ||--o{ MenuItem : "contém"

    %% ─── MESAS ──────────────────────────────────────────────────

    Table {
        Guid    Id
        Guid    TenantId
        string  Number
        int     Capacity
        string  Section
        enum    Status
    }

    Tenant ||--o{ Table : "possui"

    %% ─── HORÁRIO DE FUNCIONAMENTO ───────────────────────────────

    BusinessSchedule {
        Guid    Id
        Guid    TenantId
        list    WeeklySchedule
        list    SpecialDays
        obj     ClosingPolicy
    }

    DaySchedule {
        enum    DayOfWeek
        bool    IsOpen
        time    OpenTime
        time    CloseTime
    }

    SpecialDay {
        Guid    Id
        date    Date
        bool    IsClosed
        time    OpenTime
        time    CloseTime
        string  Reason
    }

    ClosingPolicy {
        int     GracePeriodMinutes
        bool    AllowFinishOpenTabs
        list    BlockedActions
    }

    Tenant ||--|| BusinessSchedule      : "tem horário"
    BusinessSchedule ||--o{ DaySchedule : "7 dias da semana"
    BusinessSchedule ||--o{ SpecialDay  : "dias especiais"

    %% ─── TURNO DE FUNCIONÁRIOS ──────────────────────────────────

    EmployeeSchedule {
        Guid    Id
        Guid    TenantId
        Guid    EmployeeId
        list    WeeklyShifts
    }

    DayShift {
        enum    DayOfWeek
        bool    IsWorkDay
        time    StartTime
        time    EndTime
    }

    Employee ||--|| EmployeeSchedule     : "tem turno"
    EmployeeSchedule ||--o{ DayShift     : "7 dias da semana"

    %% ─── CONFIGURAÇÕES DO TENANT ────────────────────────────────

    PaymentSettings {
        Guid    Id
        Guid    TenantId
        list    EnabledMethods
    }

    FiscalSettings {
        Guid    Id
        Guid    TenantId
        bool    NfceEnabled
        bool    AutoEmitOnTabClose
        string  Cnpj
        string  FiscalHubApiKey
    }

    PrinterSettings {
        Guid    Id
        Guid    TenantId
        list    Printers
    }

    Printer {
        Guid    Id
        string  Name
        string  IpAddressOrPort
        enum    Station
        bool    IsDefault
        bool    IsActive
    }

    IntegrationSettings {
        Guid    Id
        Guid    TenantId
        bool    DeliveryIntegrationEnabled
    }

    Tenant ||--|| PaymentSettings        : "configura pagamentos"
    Tenant ||--|| FiscalSettings         : "configura fiscal"
    Tenant ||--|| PrinterSettings        : "configura impressoras"
    Tenant ||--|| IntegrationSettings    : "configura integrações"
    PrinterSettings ||--o{ Printer       : "possui"

    %% ─── COMANDAS / PEDIDOS ─────────────────────────────────────

    Tab {
        Guid    Id
        Guid    TenantId
        int     Number
        string  CustomerName
        Guid    CurrentTableId
        string  LocationNote
        Guid    OpenedByEmployeeId
        enum    Status
        date    OpenedAt
        date    ClosedAt
    }

    Order {
        Guid    Id
        Guid    TabId
        Guid    TenantId
        Guid    TableId
        string  LocationNote
        Guid    EmployeeId
        enum    Status
        date    SubmittedAt
    }

    OrderItem {
        Guid    Id
        Guid    MenuItemId
        string  ItemName
        Money   UnitPrice
        int     Quantity
        string  Notes
        bool    RequiresKds
        enum    Status
        date    SentToKitchenAt
        date    PreparedAt
    }

    Payment {
        Guid    Id
        Guid    TabId
        Money   Amount
        enum    Method
        date    PaidAt
    }

    Tenant ||--o{ Tab         : "possui"
    Table  ||--o{ Tab         : "pode ter N"
    Employee ||--o{ Tab       : "abre"
    Tab ||--o{ Order          : "contém rounds"
    Tab ||--o{ Payment        : "pago com"
    Order ||--o{ OrderItem    : "composto de"
    MenuItem ||--o{ OrderItem : "origina (snapshot)"
```

---

## Matriz Plano × Tipo de Negócio (PlanFeatureSet)

```mermaid
graph TD
    subgraph MATRIX["Matriz gerenciada pelo AdminSaas"]
        PFS1["PlanFeatureSet\nBasic + Restaurante\n─────────────────\nfeatures.kds\nfeatures.tabs\nfeatures.reports_basic"]
        PFS2["PlanFeatureSet\nPro + Restaurante\n─────────────────\nfeatures.kds\nfeatures.tabs\nfeatures.reports_basic\nfeatures.reports_advanced\nfeatures.stock_basic\nfeatures.custom_roles"]
        PFS3["PlanFeatureSet\nBasic + Padaria\n─────────────────\nfeatures.tabs\nfeatures.daily_cash\nfeatures.reports_basic"]
        PFS4["PlanFeatureSet\nPro + Padaria\n─────────────────\nfeatures.tabs\nfeatures.daily_cash\nfeatures.reports_basic\nfeatures.reports_advanced\nfeatures.stock_basic\nfeatures.stock_advanced\nfeatures.custom_roles"]
    end

    Basic["Plan: Basic"] --> PFS1
    Basic --> PFS3
    Pro["Plan: Pro"] --> PFS2
    Pro --> PFS4
    Restaurante["BusinessType: Restaurante"] --> PFS1
    Restaurante --> PFS2
    Padaria["BusinessType: Padaria"] --> PFS3
    Padaria --> PFS4
```

---

## Fluxo de remoção de feature (Período de Carência)

```mermaid
sequenceDiagram
    actor AdminSaas
    participant PFS as PlanFeatureSet
    participant App as Application Layer
    participant Grace as FeatureGracePeriod
    actor Owner as Owner do Tenant

    AdminSaas->>PFS: RemoveFeature("features.reports_basic", graceDays=30)
    PFS-->>App: FeatureRemovedFromMatrixEvent

    App->>App: Busca todos tenants com\nPlanId + BusinessTypeId afetados

    loop Para cada tenant afetado
        App->>Grace: Create(tenantId, "features.reports_basic", 30)
        App->>Owner: Envia notificação por e-mail
        Grace-->>Grace: MarkAsNotified()
    end

    Note over Grace,Owner: Durante 30 dias o tenant ainda acessa reports

    App->>App: Job diário verifica FeatureGracePeriod expirados
    App->>App: Bloqueia acesso à feature após ExpiresAt
```

---

## Fluxo de downgrade de plano

```mermaid
sequenceDiagram
    actor AdminSaas
    participant T as Tenant
    participant App as Application Layer
    participant Role as TenantRole
    actor Owner

    AdminSaas->>T: ChangePlan(newPlanId com MaxCustomRoles=0)
    T-->>App: TenantPlanChangedEvent

    App->>App: Busca TenantRoles customizados\n(IsFromTemplate=false, IsOwnerRole=false)
    App->>App: Conta: há N cargos customizados\nNovo limite: 0

    loop Para cada cargo customizado excedente
        App->>Role: DeactivateByPlanDowngrade()
        Note right of Role: IsActive = false\nFuncionários vinculados perdem acesso
    end

    App->>Owner: Notifica com lista de funcionários\na reatribuir
```

---

## Status de Comanda, Pedido e Item

```mermaid
stateDiagram-v2
    state "Tab (Comanda)" as TAB {
        [*] --> Open : Tab.Open()
        Open --> Closed    : Tab.Close()\n(pagamentos = total)
        Open --> Cancelled : Tab.Cancel(reason)
    }

    state "Order (Round)" as ORD {
        [*] --> Open2 : tab.AddOrder()
        Open2 --> Submitted  : tab.SubmitOrder()
        Open2 --> Cancelled2 : tab.CancelOrder()
        Submitted --> InProgress : StartItemPreparation()
        InProgress --> Ready     : MarkItemReady() todos prontos
        Ready --> Delivered      : DeliverItem() todos entregues
    }

    state "OrderItem" as ITEM {
        [*] --> Pending3 : item criado
        Pending3 --> Sent3      : requiresKds=true → Submit
        Pending3 --> Delivered3 : requiresKds=false
        Pending3 --> Cancelled3 : CancelItem()
        Sent3 --> Preparing3    : KDS StartPreparing
        Preparing3 --> Ready3   : KDS MarkReady
        Ready3 --> Delivered3   : DeliverItem
    }
```
