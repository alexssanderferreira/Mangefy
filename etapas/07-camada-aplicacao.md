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
│   │   └── ValidationBehavior.cs        ← pipeline MediatR que valida antes de todo handler
│   ├── Exceptions/
│   │   ├── NotFoundException.cs
│   │   ├── ForbiddenException.cs
│   │   └── ConflictException.cs
│   ├── Interfaces/
│   │   ├── ICurrentUser.cs              ← TenantId, EmployeeId, Permissions, IsAdminSaas
│   │   ├── IUnitOfWork.cs               ← SaveChangesAsync
│   │   ├── IPasswordHasher.cs           ← Hash / Verify
│   │   ├── ITokenService.cs             ← GenerateToken + GenerateAdminSaasToken → TokenResult
│   │   └── IAdminSaasCredentials.cs     ← Email + PasswordHash do AdminSaas (lido da config)
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
| `IUnitOfWork` | `SaveChangesAsync` — persiste todas as mudanças da request |
| `IPasswordHasher` | `Hash(password)` / `Verify(password, hash)` |
| `ITokenService` | `GenerateToken(...)` e `GenerateAdminSaasToken(email)` → `TokenResult` |
| `ICurrentUser` | Fornece `TenantId`, `EmployeeId`, `Permissions`, `IsAdminSaas` do usuário autenticado |
| `IAdminSaasCredentials` | Fornece `Email` e `PasswordHash` do AdminSaas (implementado em Infrastructure via IConfiguration) |

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
3. Verifica senha via `IPasswordHasher.Verify`
4. Carrega TenantRole → obtém permissões (Owner recebe `PermissionCatalog.All`)
5. Gera token via `ITokenService.GenerateToken`
6. Chama `employee.RecordLogin()` e persiste

**Fluxo de Login (AdminSaas):**
1. Compara e-mail com `IAdminSaasCredentials.Email` → `ForbiddenException` se diferente
2. Verifica senha com `IPasswordHasher.Verify` → `ForbiddenException` se inválida
3. Gera token via `ITokenService.GenerateAdminSaasToken` com claim `isAdminSaas=true`

### Tenants (AdminSaas)
| Use Case | Descrição |
|----------|-----------|
| `CreateTenant` | Cria Tenant + OwnerRole + roles de template + Employee Owner (status PendingActivation) |
| `UpdateTenant` | Atualiza nome, timezone, slug |
| `ChangeTenantPlan` | Troca o plano do tenant |
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
| `CreateEmployee` | Cria Employee com cargo e status PendingActivation |
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
| `OpenTab` | Abre comanda (mesa opcional) |
| `SubmitOrder` | Envia pedido com itens |
| `MarkItemReady` | KDS marca item como pronto (baixa estoque automaticamente) |
| `CloseTab` | Fecha comanda com pagamentos |
| `GetOpenTabs` | Lista todas as comandas abertas do tenant |
| `GetTabById` | Retorna comanda específica com validação de tenant |

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
| `CloseCashRegister` | Fecha caixa com contagem física |
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
| `SetPasswordCommand` | ✅ |
| `AdminSaasLoginCommand` | ✅ |
| `CreateTenantCommand` | ✅ |
| `UpdateTenantCommand` | ✅ |
| `SuspendTenantCommand` | ✅ |
| `ChangeTenantPlanCommand` | ✅ |
| `CreateRoleCommand` | ✅ |
| `UpdateRolePermissionsCommand` | ✅ |
| `CreateEmployeeCommand` | ✅ |
| `UpdateEmployeeCommand` | ✅ |
| `DeactivateEmployeeCommand` | ✅ |
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

> Commands simples de status (ActivateMenu, DeactivateMenu, ConfirmReservation, MarkNoShow, RemoveMenuCategory, RemoveMenuItem, RemovePrinter) não têm validator — os IDs de rota são suficientes.

---

## Permissões do Owner no token

O `LoginCommandHandler` popula o token do **Owner** com `PermissionCatalog.All` (todas as permissões da plataforma). Funcionários comuns recebem apenas as permissões do seu `TenantRole`.
