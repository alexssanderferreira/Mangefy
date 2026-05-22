# Camada de Aplicação — Mangefy

Documentação da camada Application: CQRS, validação, casos de uso implementados.

---

## Visão Geral

A camada Application orquestra os casos de uso do sistema. Ela depende apenas do Domain — não conhece Infrastructure nem API. Usa os padrões CQRS + MediatR e FluentValidation com pipeline automático.

**Projeto:** `Mangefy.Application`  
**Framework:** .NET 8  
**Pacotes principais:** MediatR 12.4.1, FluentValidation 11.11.0

---

## Estrutura de Pastas

```
Mangefy.Application/
├── Common/
│   ├── Behaviors/
│   │   ├── ValidationBehavior.cs        ← pipeline MediatR que valida antes de todo handler
│   │   └── IdempotencyBehavior.cs       ← idempotência real: busca IdempotencyEntry por (TenantId, CommandId); retorna resposta cacheada se existir e não expirou (TTL 24h)
│   ├── Exceptions/
│   │   ├── NotFoundException.cs
│   │   ├── ForbiddenException.cs
│   │   └── ConflictException.cs
│   ├── Interfaces/
│   │   ├── ICurrentUser.cs              ← TenantId, EmployeeId, Permissions, IsAdminSaas
│   │   ├── IUnitOfWork.cs               ← SaveChangesAsync
│   │   ├── IPasswordHasher.cs           ← Hash / Verify
│   │   ├── ITokenService.cs             ← GenerateToken + GenerateAdminSaasToken → TokenResult
│   │   ├── IAdminSaasCredentials.cs     ← Email + PasswordHash do AdminSaas (lido da config)
│   │   ├── IAuditService.cs             ← LogAsync(tenantId, employeeId, isAdminSaas, action, entityType, entityId, ...)
│   │   ├── IFeatureGateService.cs       ← IsEnabledAsync / RequireAsync — verifica PlanFeatureSet + FeatureGracePeriod
│   │   └── IShiftEnforcementService.cs  ← CanOperateAsync / EnsureCanOperateAsync — Owner passa sempre; verifica TemporaryAccess e EmployeeSchedule.IsOnDutyAt
│   └── Result.cs
├── DependencyInjection.cs               ← AddApplication()
└── {Contexto}/
    ├── Commands/{UseCase}/
    │   ├── {UseCase}Command.cs
    │   ├── {UseCase}CommandHandler.cs
    │   └── {UseCase}CommandValidator.cs
    └── Queries/{UseCase}/
        ├── {UseCase}Query.cs
        ├── {UseCase}QueryHandler.cs
        └── {UseCase}Dto.cs
```

---

## Pipeline de Validação

O `ValidationBehavior<TRequest, TResponse>` é um `IPipelineBehavior` registrado globalmente. Ele executa todos os `IValidator<TRequest>` antes de chamar o handler. Se houver erros de validação, lança `ValidationException` (FluentValidation) que o middleware de API converte em HTTP 400.

```csharp
services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});
services.AddValidatorsFromAssembly(assembly);
```

---

## Interfaces de Infraestrutura

| Interface | Responsabilidade |
|-----------|-----------------|
| `IUnitOfWork` | `SaveChangesAsync` — persiste e despacha domain events |
| `IPasswordHasher` | `Hash(password)` / `Verify(password, hash)` |
| `ITokenService` | `GenerateToken(...)` e `GenerateAdminSaasToken(email)` → `TokenResult` |
| `ICurrentUser` | Fornece `TenantId`, `EmployeeId`, `Permissions`, `IsAdminSaas` do usuário autenticado |
| `IAdminSaasCredentials` | Fornece `Email` e `PasswordHash` do AdminSaas (implementado em Infrastructure via IConfiguration) |
| `IActivationTokenRepository` | CRUD de `ActivationToken` — tokens de ativação/redefinição de senha |
| `IAuditService` | `LogAsync(...)` — grava `AuditLog` para ações sensíveis (implementado em Infrastructure) |
| `IFeatureGateService` | `IsEnabledAsync` / `RequireAsync` — verifica acesso à feature por plano + carência (implementado em Infrastructure) |

---

## Casos de Uso Implementados

### Auth
| Use Case | Command/Query | Descrição |
|----------|--------------|-----------|
| Login | `LoginCommand` → `LoginResult` | Valida credenciais, verifica hash, gera JWT, registra último acesso |
| SetPassword | `SetPasswordCommand` | Define senha, ativa employee se PendingActivation (primeiro acesso) |
| AdminSaasLogin | `AdminSaasLoginCommand` → `AdminSaasLoginResult` | Valida credenciais do AdminSaas via config; gera JWT com claim `isAdminSaas=true` |

**Fluxo de Login (Employee):**
1. Busca Employee por e-mail → lança `ForbiddenException` se não existe
2. Verifica status: Inactive ou PendingActivation → `ForbiddenException`
3. Verifica senha via `IPasswordHasher.Verify` (se `PasswordHash` for nulo → credenciais inválidas)
4. Carrega TenantRole → obtém permissões (Owner recebe `PermissionCatalog.All`)
5. Gera token via `ITokenService.GenerateToken`
6. Chama `employee.RecordLogin()` e persiste

**Fluxo de Login (AdminSaas):**
1. Compara e-mail com `IAdminSaasCredentials.Email` → `ForbiddenException` se diferente
2. Verifica senha com `IPasswordHasher.Verify` → `ForbiddenException` se inválida
3. Gera token via `ITokenService.GenerateAdminSaasToken` com claim `isAdminSaas=true`

**Fluxo de SetPassword (ativação / redefinição):**
1. Busca `ActivationToken` por token string → `ForbiddenException` se não existe
2. Verifica `token.IsValid()` — não usado e não expirado → `ForbiddenException` se inválido
3. Busca Employee pelo `ActivationToken.EmployeeId`
4. Faz hash da nova senha via `IPasswordHasher.Hash` e chama `employee.ChangePassword(hash)`
5. Se `PendingActivation`, chama `employee.Activate()` → status `Active`
6. Chama `token.MarkAsUsed()` — invalida o token para uso futuro
7. Persiste employee e token

### Tenants (AdminSaas)
| Use Case | Descrição |
|----------|-----------|
| `CreateTenant` | Cria Tenant + OwnerRole + roles de template + Employee Owner (PendingActivation, sem senha) + `ActivationToken` (48h) para o Owner |
| `UpdateTenant` | Atualiza nome, timezone, slug |
| `ChangeTenantPlan` | Troca o plano do tenant; rejeita com 409 se o novo plano estiver inativo |
| `ChangeBusinessType` | Troca o tipo de negócio do tenant; valida que o novo tipo existe |
| `SuspendTenant` | Suspende ou reativa o tenant |
| `GetTenantById` | Retorna `TenantDto` |
| `ListTenants` | Lista todos os tenants (AdminSaas) |

### Roles
| Use Case | Descrição |
|----------|-----------|
| `CreateRole` | Valida `MaxCustomRoles` do plano antes de criar |
| `UpdateRolePermissions` | Atualiza lista de permissões de um cargo |
| `GetRolesByTenant` | Lista todos os cargos do tenant com permissões |

### Employees
| Use Case | Descrição |
|----------|-----------|
| `CreateEmployee` | Cria Employee (sem senha) com status PendingActivation + gera `ActivationToken` (48h). Retorna `CreateEmployeeResult(EmployeeId, ActivationToken)` |
| `UpdateEmployee` | Atualiza nome e cargo do employee |
| `DeactivateEmployee` | Desativa o employee |
| `GrantTemporaryAccess` | Owner estende acesso de um funcionário |
| `GetActiveEmployees` | Lista funcionários com turno ativo ou acesso temporário válido |
| `GetEmployeesByTenant` | Lista todos os employees do tenant (`EmployeeDto` com status, lastLoginAt) |
| `GetEmployeeById` | Retorna `EmployeeDto` de um employee específico com validação de tenant |

### Menus
| Use Case | Descrição |
|----------|-----------|
| `CreateMenu` | Cria um novo cardápio |
| `ActivateMenu` | Ativa o cardápio |
| `DeactivateMenu` | Desativa o cardápio |
| `AddMenuCategory` | Adiciona categoria ao cardápio |
| `UpdateMenuCategory` | Atualiza nome, descrição e ordem de exibição de uma categoria |
| `RemoveMenuCategory` | Remove categoria (domain lança exceção se ainda tiver itens) |
| `AddMenuItem` | Adiciona item com receita opcional |
| `UpdateMenuItem` | Atualiza nome, preço, estação e demais campos de um item |
| `RemoveMenuItem` | Remove item de uma categoria |
| `SetMenuItemStatus` | Altera status do item (Available, Unavailable, OutOfStock) |
| `SetMenuItemRecipe` | Define ficha técnica do item (lista de `StockItemId + Quantity`); exige `features.stock_basic` |
| `ClearMenuItemRecipe` | Remove ficha técnica do item |
| `GetMenusByTenant` | Lista todos os cardápios com hierarquia completa de categorias e itens |
| `GetMenuById` | Retorna cardápio específico com validação de tenant |

### Tables
| Use Case | Descrição |
|----------|-----------|
| `CreateTable` | Cria mesa com capacidade e localização |
| `UpdateTable` | Atualiza número, capacidade e seção (verifica conflito de número) |
| `SetTableStatus` | Permite marcar mesa como Available (`Release()`) ou Unavailable (`MarkAsUnavailable()`); Occupied e Reserved são gerenciados pelo sistema |
| `GetTablesByTenant` | Lista todas as mesas do tenant |

### Tabs (Comandas)
| Use Case | Descrição |
|----------|-----------|
| `OpenTab` | Abre comanda; aceita `Channel` (InPerson/Delivery/TakeAway), `DeliveryInfo?` e `ClientCommandId?` (idempotência) |
| `SubmitOrder` | Envia pedido; aceita `Modifiers` por item e `ClientCommandId?` |
| `CancelTab` | Cancela comanda com motivo; grava `AuditLog` (tab.cancelled) |
| `CloseTab` | Fecha comanda; aceita `DiscountAmount`, `ServiceFee`, `Tip`; cria `FiscalDocument` se `AutoEmitOnTabClose = true`; grava `AuditLog` quando desconto aplicado |
| `CancelOrderItem` | Cancela item com motivo opcional; grava `AuditLog` (order_item.cancelled) |
| `ReturnOrderItem` | Devolve item entregue à cozinha (`Status = Returned`); grava `AuditLog` |
| `StartItemPreparation` | KDS aceita item para preparo; exige `features.kds` |
| `MarkItemReady` | KDS marca item como pronto; exige `features.kds`; dispara `OrderReadyEvent` |
| `GetOpenTabs` | Lista todas as comandas abertas do tenant |
| `GetTabById` | Retorna comanda específica com validação de tenant |

**Event Handlers:**
| Handler | Evento | O que faz |
|---------|--------|-----------|
| `OrderReadyEventHandler` | `OrderReadyEvent` | Deduz estoque via ficha técnica de cada item da comanda; ignora itens sem receita ou sem estoque suficiente |

### AuditLogs
| Use Case | Descrição |
|----------|-----------|
| `GetAuditLogs` | Lista registros de auditoria de um tenant por período (`From`/`To`) → `IReadOnlyList<AuditLogDto>` |

### Reports
| Use Case | Descrição |
|----------|-----------|
| `GetSalesReport` | Relatório de vendas por período: receita total, descontos, taxa de serviço, gorjeta, ticket médio, receita por dia, top 20 itens, cancelamentos, breakdown por método de pagamento; exige `features.reports_basic` |
| `GetOperationalReport` | Snapshot operacional atual: comandas abertas, itens atrasados (> 15 min na cozinha), itens abaixo do mínimo de estoque; exige `features.reports_basic` |

### Fiscal
| Use Case | Descrição |
|----------|-----------|
| `CancelFiscalDocument` | Cancela documento fiscal emitido; exige `Status = Issued`; grava `AuditLog` (fiscal_document.cancelled) |
| `GetFiscalDocuments` | Lista documentos fiscais do tenant por período |

### Stock (Estoque)
| Use Case | Descrição |
|----------|-----------|
| `AddStockItem` | Adiciona item ao estoque (cria agregado Stock se não existe) |
| `RegisterPurchase` | Registra entrada de estoque |
| `AdjustInventory` | Ajuste manual de quantidade com motivo; usa `ICurrentUser.EmployeeId` para trilha de auditoria |
| `GetStockByTenant` | Lista todos os itens de estoque com flag `IsBelowMinimum` |

### DailyCash (Caixa)
| Use Case | Descrição |
|----------|-----------|
| `OpenCashRegister` | Abre caixa com valor de abertura |
| `CloseCashRegister` | Fecha caixa com lista de `MethodBalanceDto[]` (método, valorEsperado, valorContado); exige `ClosingNotes` quando há divergência |
| `RegisterWithdrawal` | Registra sangria com motivo; usa `ICurrentUser.EmployeeId` para trilha de auditoria |
| `GetCurrentCashRegister` | Retorna o caixa aberto atual com retiradas e totais |
| `GetCashRegisterHistory` | Retorna histórico de caixas por período (`From` / `To`) |

### Reservations
| Use Case | Descrição |
|----------|-----------|
| `CreateReservation` | Cria reserva (status Pending) |
| `ConfirmReservation` | Confirma a reserva |
| `CancelReservation` | Cancela a reserva com motivo obrigatório |
| `MarkNoShow` | Marca a reserva como No-Show |
| `RegisterArrival` | Registra chegada → abre Tab automaticamente |
| `GetReservationsByDate` | Lista reservas de uma data específica |

### Subscriptions (AdminSaas)
| Use Case | Descrição |
|----------|-----------|
| `CreateSubscription` | Cria assinatura do tenant |
| `GenerateInvoice` | Gera fatura |
| `ConfirmPayment` | Confirma pagamento da fatura |

### Settings — WorkforceSettings
| Use Case | Descrição |
|----------|-----------|
| `UpdateWorkforceSettings` | Atualiza `ShiftToleranceMinutes` — cria o registro se ainda não existir (upsert) |
| `GetWorkforceSettings` | Retorna `WorkforceSettingsDto(Id, ShiftToleranceMinutes)` |

### Settings — PaymentSettings
| Use Case | Descrição |
|----------|-----------|
| `UpdatePaymentSettings` | Habilita/desabilita métodos de pagamento individualmente via diff (upsert) |
| `GetPaymentSettings` | Retorna `PaymentSettingsDto(Id, IReadOnlyList<string> EnabledMethods)` |

### Settings — FiscalSettings
| Use Case | Descrição |
|----------|-----------|
| `UpdateFiscalSettings` | Habilita/desabilita NF-Ce com CNPJ e chave API; desabilita limpando os dados (upsert) |
| `GetFiscalSettings` | Retorna `FiscalSettingsDto(Id, NfceEnabled, AutoEmitOnTabClose, Cnpj)` — `FiscalHubApiKey` não é retornada por segurança |

### Settings — PrinterSettings
| Use Case | Descrição |
|----------|-----------|
| `AddPrinter` | Adiciona impressora (cria agregado se não existe — upsert) |
| `UpdatePrinter` | Atualiza nome, IP/porta e estação de uma impressora |
| `RemovePrinter` | Remove impressora pelo Id |
| `GetPrinterSettings` | Retorna `PrinterSettingsDto(Id, IReadOnlyList<PrinterDto>)` |

### Settings — TabSettings
| Use Case | Descrição |
|----------|-----------|
| `UpdateTabSettings` | Atualiza intervalo de números de comanda (`MinTabNumber`, `MaxTabNumber`) — upsert |
| `GetTabSettings` | Retorna `TabSettingsDto(Id, MinTabNumber, MaxTabNumber, TotalNumbers)` |

### Settings — BusinessSchedule
| Use Case | Descrição |
|----------|-----------|
| `UpdateBusinessSchedule` | Atualiza grade semanal, dias especiais e política de fechamento — upsert |
| `GetBusinessSchedule` | Retorna `BusinessScheduleDto` com `WeeklySchedule`, `SpecialDays` e `ClosingPolicy` |

> **Nota de implementação:** O namespace `Mangefy.Application.Settings.BusinessSchedule` conflita com o tipo de domínio `BusinessSchedule`. O handler usa o alias `DomainBusinessSchedule = Mangefy.Domain.BusinessSchedules.BusinessSchedule` para resolver o conflito.

### Settings — EmployeeSchedule
| Use Case | Descrição |
|----------|-----------|
| `UpdateEmployeeSchedule` | Define turnos semanais por dia (WorkDay/DayOff) — upsert; valida que o employee pertence ao tenant |
| `GetEmployeeSchedule` | Retorna `EmployeeScheduleDto` com `WeeklyShifts` |

> **Nota de implementação:** Idem ao BusinessSchedule — usa alias `DomainEmployeeSchedule = Mangefy.Domain.EmployeeSchedules.EmployeeSchedule`.

### Settings — ReservationSettings
| Use Case | Descrição |
|----------|-----------|
| `UpdateReservationSettings` | Atualiza limite máximo de reservas simultâneas (nullable — null = sem limite) — upsert |
| `GetReservationSettings` | Retorna `ReservationSettingsDto(Id, MaxSimultaneousReservations?)` |

### Settings — TabSettings (atualizado)
`TabSettings` agora inclui `MaxDiscountPercent` (padrão 10%) e `DiscountReasonRequiredAbove?`. Endpoint `UpdateTabSettings` aceita os novos campos.

---

## Convenções

- Handlers lançam exceções de `Common.Exceptions` (NotFoundException, ForbiddenException, ConflictException) ou `DomainException` — nunca retornam bool/null.
- `IUnitOfWork.SaveChangesAsync` é sempre a última chamada do handler.
- DTOs são records. Quando mapeiam de entidade de domínio, usam factory `static FromDomain(Entity e)`.
- Commands e Queries são records imutáveis.
- Handlers que precisam do funcionário logado injetam `ICurrentUser` (ex.: `RegisterWithdrawal`, `AdjustInventory`).
- `_currentUser.EmployeeId` é `Guid?` — handlers que passam para o domínio usam `?? Guid.Empty`.
- Settings usam padrão **upsert**: carregam o agregado; se `null`, criam via `CreateDefault(tenantId)` e chamam `AddAsync`; depois atualizam e chamam `UpdateAsync`.
- Aliases de namespace (`DomainX = Mangefy.Domain.X.X`) são usados quando o namespace da Application tem o mesmo nome que o tipo de domínio.

---

## Validators implementados

Todo Command com entrada externa tem um `AbstractValidator<T>` registrado no pipeline `ValidationBehavior`.

| Command | Validator |
|---------|-----------|
| `LoginCommand` | ✅ |
| `SetPasswordCommand` | ✅ — Token não vazio, NewPassword mínimo 8 chars |
| `AdminSaasLoginCommand` | ✅ |
| `CreateTenantCommand` | ✅ |
| `UpdateTenantCommand` | ✅ |
| `SuspendTenantCommand` | ✅ |
| `ChangeTenantPlanCommand` | ✅ |
| `CreateRoleCommand` | ✅ |
| `UpdateRolePermissionsCommand` | ✅ |
| `CreateEmployeeCommand` | ✅ — sem PasswordHash (removido) |
| `UpdateEmployeeCommand` | ✅ |
| `DeactivateEmployeeCommand` | ✅ |
| `GrantTemporaryAccessCommand` | ✅ |
| `CreateMenuCommand` | ✅ |
| `AddMenuCategoryCommand` | ✅ |
| `UpdateMenuCategoryCommand` | ✅ — Name não vazio, DisplayOrder >= 0 |
| `AddMenuItemCommand` | ✅ |
| `UpdateMenuItemCommand` | ✅ — valida `Enum.TryParse<MenuItemStation>` |
| `SetMenuItemStatusCommand` | ✅ — valida `Enum.TryParse<MenuItemStatus>` |
| `CreateTableCommand` | ✅ |
| `UpdateTableCommand` | ✅ |
| `SetTableStatusCommand` | ✅ — só aceita "Available" ou "Unavailable" |
| `CancelReservationCommand` | ✅ — Reason não vazio, max 300 chars |
| `OpenTabCommand` | ✅ |
| `SubmitOrderCommand` | ✅ |
| `CloseTabCommand` | ✅ |
| `MarkItemReadyCommand` | ✅ |
| `AddStockItemCommand` | ✅ |
| `RegisterPurchaseCommand` | ✅ |
| `AdjustInventoryCommand` | ✅ — NewQuantity >= 0, Reason não vazio |
| `OpenCashRegisterCommand` | ✅ |
| `CloseCashRegisterCommand` | ✅ |
| `RegisterWithdrawalCommand` | ✅ — Amount > 0, Reason não vazio |
| `CreateReservationCommand` | ✅ |
| `RegisterArrivalCommand` | ✅ |
| `CreateSubscriptionCommand` | ✅ |
| `GenerateInvoiceCommand` | ✅ |
| `UpdateWorkforceSettingsCommand` | ✅ |
| `UpdatePaymentSettingsCommand` | ✅ |
| `UpdateFiscalSettingsCommand` | ✅ |
| `AddPrinterCommand` | ✅ |
| `UpdatePrinterCommand` | ✅ |
| `UpdateTabSettingsCommand` | ✅ |
| `UpdateBusinessScheduleCommand` | ✅ |
| `UpdateEmployeeScheduleCommand` | ✅ |
| `UpdateReservationSettingsCommand` | ✅ |

> Commands simples de status (ActivateMenu, DeactivateMenu, ConfirmReservation, MarkNoShow, RemoveMenuCategory, RemoveMenuItem, RemovePrinter) não têm validator — os IDs de rota são suficientes.

---

## Permissões do Owner no token

O `LoginCommandHandler` popula o token do **Owner** com `PermissionCatalog.All` (todas as permissões da plataforma). Funcionários comuns recebem apenas as permissões do seu `TenantRole`.

---

## Idempotência (DB-backed)

`IIdempotentCommand` é uma interface marcadora com `Guid? ClientCommandId`. `IdempotencyBehavior<TRequest, TResponse>` está no pipeline **antes** do `ValidationBehavior`. Se `ClientCommandId` estiver preenchido:
1. Busca `IdempotencyEntry(TenantId, CommandId)` via `IIdempotencyRepository`
2. Se encontrado e não expirado (TTL 24h): deserializa e retorna `ResponseJson` sem executar o handler
3. Se não encontrado: executa o handler, serializa a resposta como JSON e persiste a entrada

Comandos com suporte: `OpenTabCommand`, `SubmitOrderCommand`, `CancelTabCommand`, `CloseTabCommand`.

---

## Feature Gate

`IFeatureGateService.RequireAsync(tenantId, featureKey)` lança `ForbiddenException` se o tenant não tem acesso. Aplicado em:

| Handler | Feature exigida |
|---------|----------------|
| `StartItemPreparationCommandHandler` | `features.kds` |
| `MarkItemReadyCommandHandler` | `features.kds` |
| `SetMenuItemRecipeCommandHandler` | `features.stock_basic` |
| `GetSalesReportQueryHandler` | `features.reports_basic` |
| `GetOperationalReportQueryHandler` | `features.reports_basic` |
| `CreateRoleCommandHandler` | `features.custom_roles` |
| `CreateMenuCommandHandler` | `features.multi_menu` (somente se já existe menu padrão) |
| `CreateReservationCommandHandler` | `features.reservations` |
| `OpenCashRegisterCommandHandler` | `features.daily_cash` |
| `ReportsController.GetAdvanced` (controller) | `features.reports_advanced` |

---

## Módulos AdminSaas (Platform)

Todos os handlers abaixo não verificam TenantId — operam sobre dados da plataforma.

### Plans
| Use Case | Descrição |
|----------|-----------|
| `CreatePlan` | Cria plano com preço, limites de mesas/itens/usuários/cargos e descrição opcional |
| `UpdatePlan` | Atualiza preço, limites e descrição (`UpdatePlanCommand` inclui `string? Description`) |
| `ActivatePlan` | Ativa plano (Status → Active) |
| `DeactivatePlan` | Desativa plano (Status → Inactive) |
| `DeletePlan` | Remove o plano permanentemente |
| `GetPlans` | Lista **todos** os planos (ativos e inativos) |

### BusinessTypes
| Use Case | Descrição |
|----------|-----------|
| `CreateBusinessType` | Cria tipo de negócio (valida nome único) |
| `UpdateBusinessType` | Atualiza nome e descrição |
| `ToggleBusinessType` | Ativa ou desativa o tipo de negócio (flag `Activate: bool`) |
| `DeleteBusinessType` | Remove permanentemente — bloqueia se tiver templates ou tenants associados |
| `AddRoleTemplate` | Adiciona template de cargo ao tipo de negócio com permissões |
| `UpdateRoleTemplate` | Atualiza nome, descrição e permissões do template |
| `ToggleRoleTemplate` | Ativa ou desativa um template de cargo |
| `DeleteRoleTemplate` | Remove template — bloqueia se `usageCount > 0` (TenantRoles criados a partir deste template) |
| `GetBusinessTypes` | Lista todos os tipos com templates, `TenantCount` e `UsageCount` por template. Injeta `ITenantRepository.CountByBusinessTypeAsync` e `ITenantRoleRepository.CountByTemplateIdAsync` |

### PlanFeatureSets
| Use Case | Descrição |
|----------|-----------|
| `UpsertPlanFeatureSet` | Cria ou atualiza a lista de features habilitadas para Plano × Tipo de Negócio; dispara eventos `FeatureAdded`/`FeatureRemoved` do domínio |
| `GetPlanFeatureSets` | Lista feature sets de um plano |

### SupplierCategories (global)
| Use Case | Descrição |
|----------|-----------|
| `CreateGlobalSupplierCategory` | Cria categoria global (TenantId = null) |
| `UpdateSupplierCategory` | Atualiza nome e descrição |
| `GetGlobalSupplierCategories` | Lista categorias globais |

### PlatformSuppliers
| Use Case | Descrição |
|----------|-----------|
| `CreatePlatformSupplier` | Cria fornecedor no catálogo global com CNPJ, website, e-mail, telefone |
| `UpdatePlatformSupplier` | Atualiza dados do fornecedor |
| `GetPlatformSuppliers` | Lista fornecedores (filtro opcional por categoria) |
