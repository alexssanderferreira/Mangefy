# Etapa 1 — Camada de Domínio (DDD)

## O que é essa camada?

O Domínio é o coração do sistema. Ele contém **todas as regras de negócio** e não depende de
nenhuma tecnologia externa (banco de dados, HTTP, framework). É aqui que vivem as entidades,
value objects, eventos de domínio e as interfaces dos repositórios.

A metodologia usada é **Domain-Driven Design (DDD)** com o padrão de **Aggregate Root**:
cada agregado protege seus próprios invariantes e nunca deixa o sistema entrar em estado inválido.

---

## Estrutura completa

```
Mangefy.Domain/
│
├── Common/                              ← Blocos de construção base
│   ├── Entity.cs                        ← Toda entidade: Id, CreatedAt, UpdatedAt, CreatedByEmployeeId, UpdatedByEmployeeId
│   ├── AggregateRoot.cs                 ← Entity + lista de Domain Events
│   ├── ValueObject.cs                   ← Igualdade por valor, não por referência
│   ├── DomainEvent.cs                   ← Base para eventos emitidos pelos agregados
│   ├── DomainException.cs               ← Exceção de regra de negócio violada
│   └── ValueObjects/
│       ├── Email.cs                     ← Valida formato, normaliza para minúsculo
│       ├── Money.cs                     ← Valor + moeda (BRL), sem negativos, 2 casas
│       ├── PhoneNumber.cs               ← Remove formatação, valida dígitos
│       └── Address.cs                   ← CEP, logradouro, número, complemento, bairro, cidade, UF
│
├── Platform/                            ← Módulo exclusivo do AdminSaas
│   ├── BusinessTypes/                   ← Tipos de negócio (Restaurante, Bar, Padaria...)
│   │   ├── BusinessType.cs              ← Aggregate Root — gerencia RoleTemplates
│   │   ├── RoleTemplate.cs              ← Template de cargo copiado no onboarding
│   │   ├── Events/
│   │   │   └── BusinessTypeCreatedEvent.cs
│   │   └── Repositories/
│   │       └── IBusinessTypeRepository.cs
│   │
│   └── Features/                        ← Matriz Plano × Tipo de Negócio
│       ├── FeatureCatalog.cs            ← Catálogo imutável de features da plataforma
│       ├── PlanFeatureSet.cs            ← Define features ativas por combinação Plan+BusinessType
│       ├── FeatureGracePeriod.cs        ← Carência de 30 dias ao remover feature de tenant ativo
│       ├── Events/
│       │   ├── FeatureAddedToMatrixEvent.cs
│       │   └── FeatureRemovedFromMatrixEvent.cs
│       └── Repositories/
│           ├── IPlanFeatureSetRepository.cs
│           └── IFeatureGracePeriodRepository.cs
│
├── SupplierCategories/              ← Ramos de atuação de fornecedores
│   ├── SupplierCategory.cs          ← Global (TenantId=null, AdminSaas) ou exclusiva do tenant
│   └── Repositories/ISupplierCategoryRepository.cs
│
├── Suppliers/                       ← Catálogo global de fornecedores
│   ├── PlatformSupplier.cs          ← Fornecedor da plataforma — somente leitura para o tenant
│   └── Repositories/IPlatformSupplierRepository.cs
│
└── Platform/Subscriptions/          ← Assinaturas e faturas dos tenants (gestão AdminSaas)
    ├── Subscription.cs              ← Aggregate Root — faturas, confirmação de pagamento, atraso
    ├── Invoice.cs                   ← Entity — fatura com status Pending | Paid | Overdue
    ├── InvoiceStatus.cs
    ├── Events/
    │   ├── InvoicePaidEvent.cs
    │   └── InvoiceOverdueEvent.cs   ← disparado por job agendado ao detectar vencimento
    └── Repositories/ISubscriptionRepository.cs

├── Plans/                               ← Planos de assinatura
│   ├── Plan.cs                          ← Limites: mesas, itens, usuários, cargos customizados
│   ├── PlanStatus.cs                    ← Active | Inactive
│   └── Repositories/IPlanRepository.cs
│
├── Tenants/                             ← Cada estabelecimento é um Tenant
│   ├── Tenant.cs                        ← Nome, slug, e-mail, endereço, plano, tipo de negócio, status
│   ├── TenantStatus.cs                  ← Active | Suspended | Cancelled | TrialPeriod
│   ├── Events/
│   │   ├── TenantCreatedEvent.cs        ← inclui BusinessTypeId
│   │   ├── TenantPlanChangedEvent.cs    ← usado para tratar downgrade de plano
│   │   ├── TenantSuspendedEvent.cs
│   │   └── TenantCancelledEvent.cs
│   └── Repositories/ITenantRepository.cs
│
├── Roles/                               ← Sistema de cargos por tenant (RBAC)
│   ├── PermissionCatalog.cs             ← Catálogo imutável de permissões da plataforma
│   ├── TenantRole.cs                    ← Cargo do tenant: Owner | template | customizado
│   ├── Events/TenantRoleCreatedEvent.cs
│   └── Repositories/ITenantRoleRepository.cs
│
├── Employees/                           ← Funcionários do restaurante
│   ├── Employee.cs                      ← Nome, e-mail, senha (hash), cargo único, status
│   ├── EmployeeStatus.cs                ← Active | Inactive | PendingActivation
│   ├── Events/EmployeeCreatedEvent.cs
│   └── Repositories/IEmployeeRepository.cs
│
├── Menus/                               ← Cardápio do restaurante (suporta múltiplos por tenant)
│   ├── Menu.cs                          ← Aggregate Root — IsDefault, IsActive, Schedule opcional
│   ├── MenuSchedule.cs                  ← ValueObject — dias da semana + faixa de horário de vigência
│   ├── MenuCategory.cs                  ← Categoria (Entradas, Bebidas, Sobremesas...)
│   ├── MenuItem.cs                      ← Item com preço, imagem, flag KDS, setor e ficha técnica
│   ├── MenuItemStatus.cs                ← Available | Unavailable | OutOfStock
│   ├── MenuItemStation.cs               ← Kitchen | Bar | Custom
│   ├── RecipeIngredient.cs              ← ValueObject — StockItemId + Quantidade (ficha técnica)
│   └── Repositories/IMenuRepository.cs  ← GetDefaultByTenantAsync + GetAllByTenantAsync
│
├── Tables/                              ← Mesas do estabelecimento
│   ├── Table.cs                         ← Número, capacidade, setor, status
│   ├── TableStatus.cs                   ← Available | Occupied | Reserved | Unavailable
│   ├── Events/
│   │   ├── TableOccupiedEvent.cs
│   │   └── TableReleasedEvent.cs
│   └── Repositories/ITableRepository.cs
│
├── BusinessSchedules/                   ← Horário de funcionamento do estabelecimento
│   ├── BusinessSchedule.cs              ← Aggregate Root — grade semanal + dias especiais + política
│   ├── DaySchedule.cs                   ← ValueObject — horário de um dia da semana
│   ├── SpecialDay.cs                    ← Entity — feriado ou dia com horário especial
│   ├── ClosingPolicy.cs                 ← ValueObject — tolerância e ações bloqueadas ao fechar
│   └── Repositories/IBusinessScheduleRepository.cs
│
├── EmployeeSchedules/                   ← Turno semanal por funcionário
│   ├── EmployeeSchedule.cs              ← Aggregate Root — grade semanal de um funcionário
│   ├── DayShift.cs                      ← ValueObject — turno de um dia da semana
│   └── Repositories/IEmployeeScheduleRepository.cs
│
├── Settings/                            ← Configurações do tenant (pagamento, fiscal, impressoras, integrações, comandas, reservas)
│   ├── PaymentSettings.cs               ← Métodos de pagamento habilitados pelo Owner
│   ├── FiscalSettings.cs                ← NFC-e opcional (integração real é trabalho futuro)
│   ├── PrinterSettings.cs               ← Aggregate Root — múltiplas impressoras por estação
│   ├── Printer.cs                       ← Entity — impressora térmica com IP/porta e estação
│   ├── IntegrationSettings.cs           ← Estrutura reservada para integração com delivery (futuro)
│   ├── TabSettings.cs                   ← Intervalo de números de comandas físicas reutilizáveis
│   ├── ReservationSettings.cs           ← Limite máximo de reservas simultâneas (nullable = sem limite)
│   │                                       CreateDefault(tenantId) / UpdateLimit(int? max)
│   ├── WorkforceSettings.cs             ← Tolerância de atraso de turno em minutos (ShiftToleranceMinutes)
│   ├── Repositories/IPaymentSettingsRepository.cs
│   ├── Repositories/IFiscalSettingsRepository.cs
│   ├── Repositories/IPrinterSettingsRepository.cs
│   ├── Repositories/IIntegrationSettingsRepository.cs
│   ├── Repositories/ITabSettingsRepository.cs
│   ├── Repositories/IReservationSettingsRepository.cs
│   └── Repositories/IWorkforceSettingsRepository.cs
│
├── Stock/                               ← Controle de estoque (ingredientes e produtos)
│   ├── Stock.cs                         ← Aggregate Root — 1 por tenant, global com filtro por setor
│   ├── StockItem.cs                     ← Entity — item com qty, mínimo, custo, fornecedor, setor
│   ├── StockMovement.cs                 ← Entity — entrada/saída com tipo e motivo
│   ├── Supplier.cs                      ← Aggregate Root — fornecedor do tenant (manual ou do catálogo)
│   ├── StockUnit.cs                     ← Unit | Kg | Gram | Liter | Milliliter
│   ├── StockMovementType.cs             ← Purchase | Sale | ManualConsumption | Loss | InventoryAdjustment
│   ├── Events/StockLowEvent.cs          ← Disparado ao atingir ou ultrapassar o estoque mínimo
│   └── Repositories/
│       ├── IStockRepository.cs
│       └── ISupplierRepository.cs
│
├── DailyCash/                           ← Fechamento de caixa diário
│   ├── CashRegister.cs                  ← Aggregate Root — abertura, sangrias e fechamento com contagem
│   ├── CashWithdrawal.cs                ← Entity — sangria (retirada de dinheiro do caixa)
│   ├── CashRegisterStatus.cs            ← Open | Closed
│   ├── Events/
│   │   ├── CashRegisterOpenedEvent.cs
│   │   └── CashRegisterClosedEvent.cs   ← inclui valor esperado, contado e diferença
│   └── Repositories/ICashRegisterRepository.cs
│
├── Reservations/                        ← Reservas de mesa
│   ├── Reservation.cs                   ← Aggregate Root — cliente, data, mesa, status
│   ├── ReservationStatus.cs             ← Pending | Confirmed | Arrived | Cancelled | NoShow
│   ├── Events/
│   │   ├── ReservationCreatedEvent.cs
│   │   └── ReservationArrivedEvent.cs   ← dispara abertura automática de comanda
│   └── Repositories/IReservationRepository.cs
│
└── Tabs/                                ← Comandas e pedidos (núcleo operacional)
    ├── Tab.cs                           ← Aggregate Root — Comanda (por pessoa)
    ├── TabStatus.cs                     ← Open | Closed | Cancelled
    ├── Order.cs                         ← Pedido (round enviado à cozinha)
    ├── OrderStatus.cs                   ← Open→Submitted→InProgress→Ready→Delivered|Cancelled
    ├── OrderItem.cs                     ← Item com snapshot de nome e preço
    ├── OrderItemStatus.cs               ← Pending→Sent→Preparing→Ready→Delivered|Cancelled
    ├── Payment.cs                       ← Forma de pagamento (múltiplas por comanda)
    ├── PaymentMethod.cs                 ← Cash | CreditCard | DebitCard | Pix | Other
    ├── Events/
    │   ├── TabOpenedEvent.cs
    │   ├── TabClosedEvent.cs
    │   ├── TabCancelledEvent.cs
    │   ├── OrderSubmittedEvent.cs
    │   └── OrderReadyEvent.cs
    └── Repositories/ITabRepository.cs
```

---

## Catálogo de Features (FeatureCatalog)

Features controladas pela matriz Plano × Tipo de Negócio:

| Feature Key                  | Descrição                                  |
|------------------------------|--------------------------------------------|
| `features.kds`               | Tela Kitchen Display System                |
| `features.tabs`              | Gestão de comandas                         |
| `features.multi_menu`        | Múltiplos cardápios (café, almoço...)      |
| `features.stock_basic`       | Controle básico de estoque                 |
| `features.stock_advanced`    | Estoque avançado com alertas               |
| `features.daily_cash`        | Fechamento de caixa diário                 |
| `features.reports_basic`     | Relatórios essenciais                      |
| `features.reports_advanced`  | Analytics avançado                         |
| `features.reservations`      | Gestão de reservas de mesa                 |
| `features.delivery`          | Módulo de delivery                         |
| `features.custom_roles`      | Criação de cargos customizados             |

---

## Catálogo de Permissões (PermissionCatalog)

Permissões controladas por cargo (`TenantRole`):

| Permissão                 | Descrição                                  |
|---------------------------|--------------------------------------------|
| `orders.read`             | Ver pedidos e KDS                          |
| `orders.create`           | Criar pedidos e adicionar itens            |
| `orders.update_status`    | Alterar status via KDS                     |
| `orders.cancel`           | Cancelar pedidos e itens                   |
| `tabs.read`               | Ver comandas                               |
| `tabs.create`             | Abrir nova comanda                         |
| `tabs.close`              | Fechar comanda e registrar pagamento       |
| `tabs.cancel`             | Cancelar comanda                           |
| `menu.read`               | Ver cardápio                               |
| `menu.manage`             | Criar/editar/remover categorias e itens    |
| `tables.read`             | Ver mesas                                  |
| `tables.manage`           | Criar/editar/desativar mesas               |
| `employees.read`          | Ver funcionários                           |
| `employees.manage`        | Criar/editar/desativar funcionários        |
| `roles.read`              | Ver cargos                                 |
| `roles.manage`            | Criar/editar/definir permissões de cargos  |
| `reports.read`            | Ver relatórios e analytics                 |
| `settings.manage`         | Configurar dados do restaurante            |

---

## Decisões de design

### 1. Validação dentro da entidade
Regras de negócio ficam na própria entidade via `DomainException`.
O sistema nunca persiste estado inválido.

### 2. Factory Methods como único ponto de entrada
Construtores são `private`. Criação sempre por `Create()`, `Open()` ou `CreateOwner()`.

### 3. Aggregate Root protege seus filhos
- `Tab` → `Order` → `OrderItem` e `Payment` (tudo passa pela Tab)
- `Menu` → `MenuCategory` → `MenuItem`
- `BusinessType` → `RoleTemplate`

### 4. Snapshot de preço e nome no OrderItem
Alterações futuras no cardápio não afetam pedidos históricos.

### 5. Template de cargo é uma cópia (snapshot)
No onboarding, `RoleTemplate` vira `TenantRole` independente com `IsFromTemplate=true`.
Alterações no template não afetam tenants já criados.

### 6. Dois catálogos distintos
- `PermissionCatalog` — o que cada funcionário pode fazer (granular, por ação)
- `FeatureCatalog` — quais módulos/telas o tenant acessa (por plano + tipo de negócio)

### 7. Período de carência na remoção de feature
`FeatureGracePeriod` garante 30 dias de transição quando o AdminSaas remove uma feature
da matriz. Criado pela Application layer via `FeatureRemovedFromMatrixEvent`.

### 8. Horário de funcionamento com dias especiais e política de fechamento
`BusinessSchedule` mantém a grade semanal (7 `DaySchedule`) e uma lista de `SpecialDay`
(feriados e dias com horário diferente). `SpecialDay` tem prioridade sobre a grade semanal.
`ClosingPolicy` define o período de tolerância (minutos) e as ações bloqueadas durante esse período.

### 10. Rastreabilidade de autoria
`Entity` base tem `CreatedByEmployeeId` e `UpdatedByEmployeeId` (`Guid?`).
Nulo para entidades da plataforma (AdminSaas). Apenas entidades do tenant rastreiam o funcionário autor.

### 11. Numeração de comandas físicas reutilizáveis
`TabSettings` define o intervalo (ex: 1–80). O número é atribuído pelo funcionário ao abrir a comanda.
Ao fechar, o número volta imediatamente ao pool de disponíveis.
A Application layer consulta comandas abertas para determinar números livres.

### 12. Múltiplos cardápios por tenant
`Menu` tem `IsDefault`, `IsActive` e `MenuSchedule?` (dias + horário de vigência).
O cardápio padrão (`IsDefault=true`) é sempre visível. Cardápios adicionais podem ter vigência automática
por horário ou ser ativados/desativados manualmente. Vários podem estar ativos simultaneamente.
`IsVisibleAt(day, time)` resolve quais cardápios exibir em determinado momento.

### 13. Fechamento de caixa
`CashRegister` registra abertura com valor inicial, sangrias durante o turno e fechamento com contagem física.
`Close()` calcula `ExpectedAmount = abertura + vendas em dinheiro − sangrias` e registra a diferença.
`CashRegisterClosedEvent` notifica a Application layer para gerar o relatório do dia.

### 14. Reservas de mesa
`Reservation` flui: Pending → Confirmed → Arrived | Cancelled | NoShow.
`RegisterArrival(tabId)` dispara `ReservationArrivedEvent` para a Application layer abrir a comanda automaticamente.

### 15. Estoque global com filtro por setor
`Stock` é único por tenant (global). `StockItem` tem `Station` (Kitchen, Bar, Custom) para filtros.
A baixa é automática via ficha técnica (`RecipeIngredient` no `MenuItem`) ao vender um item.
`StockLowEvent` é disparado quando `CurrentQuantity <= MinimumQuantity`.

### 16. Dois níveis de fornecedor
`PlatformSupplier` (AdminSaas) é o catálogo global somente leitura para o tenant.
`Supplier` (tenant) pode referenciar um `PlatformSupplier` ou ser cadastrado manualmente.
Fornecedores manuais são totalmente editáveis; os do catálogo permitem apenas campos exclusivos do tenant (representante, notas).

### 17. Categorias de fornecedor com dois níveis
`SupplierCategory` com `TenantId = null` = global (AdminSaas). Com `TenantId` preenchido = exclusiva do tenant.

### 18. Configurações separadas por responsabilidade
Cada grupo de configuração é um Aggregate Root independente com `TenantId`. Todos usam padrão **upsert** na Application: carrega por `TenantId`; se `null`, cria via `CreateDefault(tenantId)` e chama `AddAsync`; depois atualiza e chama `UpdateAsync`.

| Aggregate | Responsabilidade |
|---|---|
| `PaymentSettings` | Métodos de pagamento habilitados (`EnableMethod` / `DisableMethod`) |
| `FiscalSettings` | NFC-e opcional (`EnableNfce` / `DisableNfce`) — integração com hub fiscal é trabalho futuro |
| `PrinterSettings` | Múltiplas impressoras por estação (`AddPrinter`, `UpdatePrinter`, `RemovePrinter`) |
| `IntegrationSettings` | Estrutura reservada para delivery (trabalho futuro) |
| `TabSettings` | Intervalo de números de comandas físicas reutilizáveis (`UpdateRange(min, max)`) |
| `WorkforceSettings` | Tolerância de atraso de turno em minutos (`ShiftToleranceMinutes`) |
| `ReservationSettings` | Limite máximo de reservas simultâneas, nullable (`UpdateLimit(int? max)`) |

---

## Fluxo de onboarding de um novo tenant

```
1. AdminSaas cria Tenant { PlanId, BusinessTypeId }
      → TenantCreatedEvent disparado

2. Application layer processa TenantCreatedEvent:
   a. Cria TenantRole "Dono" (IsOwnerRole=true)
   b. Para cada RoleTemplate ativo do BusinessType:
      → Cria TenantRole com snapshot das permissões (IsFromTemplate=true, TemplateId)
   c. Cria Employee do dono (IsOwner=true, Status=Active)

3. Owner recebe e-mail com link de primeiro acesso

4. Owner configura o restaurante:
   ├─ Edita permissões dos cargos padrão (opcional)
   ├─ Cria cargos customizados (se o plano permitir)
   ├─ Cadastra funcionários e vincula a cargos
   ├─ Configura o cardápio
   └─ Cadastra as mesas

5. Após trial → AdminSaas ativa o plano pago → Tenant.Status = Active
```

---

## Próximas etapas

| Etapa | Camada         | O que será feito                                                    |
|-------|----------------|---------------------------------------------------------------------|
| 02    | Application    | Use Cases (Commands/Queries), validações de input (FluentValidation) |
| 03    | Infrastructure | EF Core, mapeamentos, repositórios, migrações                       |
| 04    | API            | Controllers REST, JWT, middleware de tenant, autorização por feature e permissão |
| 05    | Real-time      | SignalR Hub para KDS e notificações ao garçom                       |
| 06    | Frontend       | Angular — telas por perfil de funcionário                           |
