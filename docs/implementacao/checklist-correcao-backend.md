# Checklist de Correção do Backend

> Criado em: 2026-05-13  
> Última atualização: 2026-05-13  
> Estado: build com êxito, 0 erros, 0 avisos.

---

## 1. Migrations e EF Core

- [x] Migrations SQL Server removidas (incompatíveis com PostgreSQL)
- [x] Migration `InitialPostgres` gerada para PostgreSQL (Npgsql 8) cobrindo todo o modelo:
  - [x] Tabela `ActivationTokens`
  - [x] Tabela `ReservationSettings`
  - [x] Tabela `AuditLogs`
  - [x] Tabela `FiscalDocuments`
  - [x] Tabela `IdempotencyEntries`
  - [x] Tabela `Devices`
  - [x] Tabela `OperationalSessions`
  - [x] Tabela `PrintJobs`
  - [x] Tabela `CashSupplies`
  - [x] Tabela `CashMethodBalances`
  - [x] Tabela `MenuItemPriceHistory`
  - [x] Coluna `RowVersion` em `Tabs`, `CashRegisters`, `Stocks`, `Employees` (bytea + rowVersion: true)
  - [x] Colunas `Channel`, `DeliveryInfo`, `DiscountAmount`, `ServiceFee`, `Tip` em `Tabs`
  - [x] Colunas `ChangeGiven`, `ExternalReference` em `TabPayments`
  - [x] Colunas KDS/cancelamento em `OrderItems`
  - [x] Colunas de preço promocional em `MenuItems`
  - [x] Colunas `MaxDiscountPercent`, `DiscountReasonRequiredAbove` em `TabSettings`
  - [x] `Employees.PasswordHash` nullable
  - [x] Índice único filtrado `{TenantId, Number} WHERE "Status" = 'Open'` em `Tabs` (PostgreSQL)
  - [x] Índice único `{TenantId, Email}` em `Employees` via SQL puro na migration
- [x] `MangefyDbContextModelSnapshot` atualizado pela geração da migration

> **Nota**: migration gerada com `dotnet ef migrations add InitialPostgres`. Para aplicar: requer PostgreSQL rodando com credenciais configuradas.

## 2. Onboarding de Tenant

- [x] `CreateTenantCommandHandler` deve criar junto ao tenant:
  - [x] Menu padrão (padrão ativo, IsDefault = true)
  - [x] BusinessSchedule padrão
  - [x] PaymentSettings padrão (PIX + Dinheiro habilitados por default)
  - [x] FiscalSettings padrão (NfceEnabled = false)
  - [x] PrinterSettings padrão (vazio)
  - [x] TabSettings padrão (MinTabNumber=1, MaxTabNumber=50, MaxDiscountPercent=10)
  - [x] IntegrationSettings padrão
  - [x] ReservationSettings padrão
  - [x] Stock vazio
  - [x] WorkforceSettings padrão (ShiftToleranceMinutes=15)
- [x] Manter retorno do token de ativação ou documentar que envio de e-mail é futuro

## 3. Domain Events

- [x] Implementar handler `ReservationArrivedEventHandler`: abertura automática implementada em `RegisterArrivalCommandHandler`; handler é informativo (reservado para notificações futuras)
- [x] Implementar handler `TabOpenedEventHandler`: ocupar a mesa (`Table.Occupy()`)
- [x] Implementar handler `TabClosedEventHandler`: liberar a mesa se não restarem comandas abertas na mesma mesa
- [x] Implementar handler `TabCancelledEventHandler`: idem ao fechamento
- [x] Implementar handler `TenantPlanChangedEventHandler`: tratar downgrade desativando cargos customizados excedentes
- [x] Documentar `StockLowEvent` como preparatório sem notificação externa real
- [x] Documentar `TenantCreatedEvent` como sem handler (onboarding inline no command handler)

## 4. Autenticação e Tenant Ativo

- [x] `LoginCommandHandler`: carregar o Tenant do funcionário
- [x] `LoginCommandHandler`: bloquear login se tenant estiver `Suspended` ou `Cancelled`
- [x] `LoginCommandHandler`: permitir login apenas para tenant `Active` ou `TrialPeriod`

## 5. Numeração de Comandas

- [x] `TabRepository.GetNextNumberAsync`: consultar `TabSettings` para obter `MinTabNumber` e `MaxTabNumber`
- [x] Escolher número disponível dentro do intervalo (reutilizar números de comandas fechadas/canceladas)
- [x] Impedir abertura quando não houver número disponível no intervalo
- [x] Garantir unicidade de número aberto por tenant (índice ou validação antes de salvar)
- [x] `OpenTabCommandHandler`: injetar `ITabSettingsRepository` e usar lógica corrigida

## 6. Fechamento de Comanda e Pagamentos

- [x] `CloseTabCommandHandler`: consultar `PaymentSettings` e rejeitar métodos desabilitados
- [x] `CloseTabCommandHandler`: verificar `DiscountReasonRequiredAbove` — se desconto exceder o limite, exigir `DiscountReason` no request
- [x] `CloseTabCommand`: adicionar campo `DiscountReason`

## 7. SubmitOrder e Snapshot Real do Cardápio

- [x] `SubmitOrderCommandHandler`: receber apenas `MenuItemId`, `Quantity`, `Notes`, `Modifiers` do request
- [x] Buscar o item real no cardápio do tenant via `IMenuRepository`
- [x] Validar que o item existe e pertence ao tenant
- [x] Validar que o item está `Available`
- [x] Usar nome, preço efetivo, station e `RequiresKds` do domínio
- [x] `SubmitOrderCommand`: remover campos `ItemName`, `UnitPrice`, `RequiresKds` do command item
- [x] `SubmitOrderCommandValidator`: ajustar conforme novos campos

## 8. TabSettings e Política de Desconto

- [x] `UpdateTabSettingsCommandHandler`: aceitar e persistir `MaxDiscountPercent` e `DiscountReasonRequiredAbove`
- [x] `UpdateTabSettingsCommand`: adicionar campos `MaxDiscountPercent` e `DiscountReasonRequiredAbove`
- [x] DTO de leitura (`GetTabSettings`) retornar `MaxDiscountPercent` e `DiscountReasonRequiredAbove`
- [x] `SettingsController`: aceitar/retornar esses campos no endpoint de tab settings

## 9. Employee Email Por Tenant

- [x] `IEmployeeRepository`: adicionar `ExistsByEmailAsync(Guid tenantId, string email)`
- [x] `EmployeeRepository`: implementar versão com tenantId
- [x] `EmployeeConfiguration`: índice único em `{ TenantId, Email }` em vez de apenas `Email`
- [x] Login: usa `TenantSlug + email` — `LoginCommandHandler` resolve tenant por slug, depois `GetByEmailInTenantAsync(tenantId, email)` — suporta email igual em tenants diferentes (CA-053)
- [x] Handlers que criam employee: usar `ExistsByEmailInTenantAsync(tenantId, email)` — `CreateEmployeeCommandHandler` corrigido

> **Decisão (auditoria 21-E)**: Login exige `tenantSlug + email + password`. `LoginCommandHandler` resolve tenant por slug, verifica status, depois busca employee por `(tenantId, email)`. Suporta mesmo email em tenants distintos conforme CA-053.

## 10. Turno, Timezone e Tolerância

- [x] `ShiftEnforcementService`: carregar timezone do tenant (`ITenantRepository`)
- [x] Converter `DateTime.UtcNow` para horário local do tenant via `TimeZoneInfo.ConvertTimeBySystemTimeZoneId`
- [x] Carregar `WorkforceSettings.ShiftToleranceMinutes` e aplicar tolerância no fim do turno
- [x] Documentar bloqueio global de middleware por turno como pendente — documentado em `02-pendencias-e-futuro.md` com decisão de design (filtro opt-in, não middleware global)

## 11. Feature Gates

- [x] `CreateRoleCommandHandler`: verificar feature gate `features.custom_roles` via `IFeatureGateService`
- [x] `CreateMenuCommandHandler`: verificar feature gate `features.multi_menu` quando já existe menu padrão
- [x] `CreateReservationCommandHandler`: verificar feature gate `features.reservations`
- [x] `OpenCashRegisterCommandHandler`: verificar feature gate `features.daily_cash`
- [x] Atualizar docs caso alguma feature seja decidida como não restrita por plano — decisões registradas na tabela de decisões do `02-pendencias-e-futuro.md`

## 12. RBAC e Permissões

- [x] `UpdateRolePermissionsCommandHandler`: impedir escalada — usuário com `roles.manage` só pode atribuir permissões que ele próprio possui (exceto Owner)
- [x] `TabsController`: endpoint de cancelar comanda usa permissão `tabs.cancel` (corrigido de `tabs.close`)
- [x] Verificar que cancelar item usa `orders.cancel` — `TabsController.CancelItem` corrigido de `orders.update_status` para `orders.cancel`; o handler verifica permissões granulares por status internamente
- [x] Garantir que todas as permissões do `PermissionCatalog` estejam nas docs — seção "Catálogo de Permissões" adicionada em `03-camada-api.md`

## 13. Downgrade de Plano

- [x] `ChangeTenantPlanCommandHandler`: detectar downgrade (novo plano tem `MaxCustomRoles` menor)
- [x] Desativar cargos customizados excedentes de forma determinística (critério: mais recentemente criados, preservando Owner e roles de template)
- [x] Documentar: cargos desativados no downgrade não são retornados no response; podem ser consultados via `GET /roles` após a operação. Decisão registrada em `02-pendencias-e-futuro.md`

## 14. ReservationSettings

- [x] `CreateReservationCommandHandler`: consultar `ReservationSettings.MaxSimultaneousReservations`
- [x] Se nulo, permitir ilimitado
- [x] Se houver limite, contar reservas no mesmo período (mesma data e horário) e bloquear se atingido
- [x] Documentar definição exata de "simultâneas" — registrado em `02-pendencias-e-futuro.md`: mesmo tenant, mesma data, mesmo horário, status ≠ Cancelled/NoShow

## 15. Controllers Fora do Padrão CQRS

- [x] `DevicesController`: criar Commands/Queries (`RegisterDevice`, `DeactivateDevice`, `GetDevices`) e respectivos Handlers
- [x] `OperationalSessionsController`: criar Commands/Queries (`StartSession`, `EndSession`, `GetActiveSessions`) e respectivos Handlers
- [x] `PrintJobsController`: criar Commands/Queries (`GetPendingJobs`, `ReprintJob`, `MarkAsPrinted`) e respectivos Handlers
- [x] Controllers devem apenas injetar `ISender` e delegar

## 16. CashRegister Difference

- [x] `CashRegister.Difference`: corrigido para `decimal?` — diferença pode ser negativa (falta) ou positiva (sobra)
- [x] `CashRegisterDto`: corrigido para mapear `decimal?` diretamente (sem `.Amount`)

## 17. Rastreabilidade

- [x] Definir abordagem: hook no `UnitOfWork.SaveChangesAsync` via `ICurrentUser`
- [x] Implementar `ApplyAuditInfo()` no `UnitOfWork`: para `Added` → `SetCreatedByEmployee`, para `Modified` → `SetUpdatedByEmployee`
- [x] Métodos `SetCreatedByEmployee` e `SetUpdatedByEmployee` adicionados à base `Entity`
- [x] Não preenche para entidades sem `EmployeeId` (AdminSaas não tem sessão de funcionário)

## 18. Módulos AdminSaas Faltantes

- [x] Criar `PlansController` com endpoints: listar, criar, atualizar, ativar/desativar plano
- [x] Criar `BusinessTypesController` com endpoints: listar, criar, atualizar
- [x] Criar `RoleTemplatesController` com endpoints: adicionar, atualizar, ativar/desativar template (sub-resource de BusinessType)
- [x] Criar `PlanFeatureSetsController` com endpoints: listar, criar/atualizar feature set por plano+businessType
- [x] Criar `SupplierCategoriesController` (global) com endpoints AdminSaas
- [x] Criar `PlatformSuppliersController` com endpoints AdminSaas
- [x] Criar Commands/Queries/Handlers correspondentes para cada módulo
- [x] Todos os controllers protegidos com `[RequireAdminSaas]`

## 19. Relatórios Avançados

- [x] `ReportsController`: endpoint `GET /reports/advanced` criado com verificação de `features.reports_advanced`
- [x] Payload retorna estrutura preparatória com nota de desenvolvimento futuro (RF-054)

## 20. Documentação

- [x] Atualizar `docs/implementacao/checklist-correcao-backend.md` (este arquivo)
- [x] Atualizar `docs/implementacao/01-camada-aplicacao.md` — módulos AdminSaas, feature gates, rastreabilidade
- [x] Atualizar `docs/implementacao/02-camada-infraestrutura.md` — handlers de eventos, rastreabilidade, `ExistsByEmailInTenantAsync`
- [x] Atualizar `docs/implementacao/03-camada-api.md` — controllers AdminSaas, catálogo de permissões, `/reports/advanced`
- [x] Atualizar `docs/decisoes/02-pendencias-e-futuro.md` — status dos handlers, downgrade, reservas simultâneas, middleware turno
- [x] `docs/dominio/03-requisitos-e-aceite.md` — arquivo existe e contém CA-182/183 que definem chegada automática de Tab

---

## 21. Correções da Auditoria Atual

### A. Domain Events — Alterações de handlers não persistidas
- [x] `UnitOfWork.SaveChangesAsync`: adicionar segundo `SaveChangesAsync` após publicar todos os domain events para persistir mudanças feitas pelos handlers (ex.: `Table.Occupy()` em `TabOpenedEventHandler`)
- [x] Remover comentário incorreto de `TabOpenedEventHandler` que afirmava que o UoW seria commitado externamente

### B. Repositórios AdminSaas sem implementação em Infrastructure
- [x] Criar `PlatformSupplierRepository` implementando `IPlatformSupplierRepository`
- [x] Criar `SupplierCategoryRepository` implementando `ISupplierCategoryRepository`
- [x] Registrar ambos em `DependencyInjection.cs`

### C. Migrations pendentes
- [x] Índice único `{TenantId, Number}` filtrado para tabs abertas em `TabConfiguration` (incluído na migration `InitialPostgres`)

### D. Fluxo de chegada de reserva (RegisterArrival)
- [x] `RegisterArrivalCommand` remover campo `TabId` obrigatório; substituído por `EmployeeId`
- [x] `RegisterArrivalCommandHandler` abre Tab automaticamente via `ISender.Send(OpenTabCommand)` conforme CA-182/183; retorna `tabId`
- [x] `RegisterArrivalCommandValidator` atualizado
- [x] Controller `ReservationsController.RegisterArrival` atualizado (body com `EmployeeId`; retorna `{ tabId }`)
- [x] `ReservationArrivedEventHandler` atualizado para handler informativo (Tab já aberta antes do evento)

### E. Login com e-mail ambíguo (email único por tenant, login global)
- [x] `LoginCommand` adicionar campo `TenantSlug`
- [x] `LoginCommandHandler` busca tenant por slug, verifica status, depois employee por `(tenantId, email)` via `GetByEmailInTenantAsync`
- [x] Adicionar `GetByEmailInTenantAsync(tenantId, email)` em `IEmployeeRepository` e `EmployeeRepository`
- [x] `LoginCommandValidator` valida `TenantSlug` não vazio
- [x] `AuthController.Login` recebe `TenantSlug` no body (record binding automático)
- [x] Decisão documentada: login requer `tenantSlug + email + password`; suporta mesmo email em tenants diferentes

### F. Concorrência na numeração de comandas
- [x] `TabConfiguration` adicionar índice único filtrado `{TenantId, Number}` WHERE `Status = 'Open'` para proteção via banco (migration pendente de aplicar)
- [x] Documentado: banco garante unicidade com `ConflictException` (409); `GetNextAvailableNumberAsync` é proteção em código (soft-lock)

### G. Descrição incorreta de `GetNextAvailableNumberAsync`
- [x] Documentado: `GetNextAvailableNumberAsync` recebe `min`/`max` como parâmetros; `OpenTabCommandHandler` consulta `TabSettings` e passa o intervalo — design correto e intencional
- [x] Checklist item 5 revisado: repositório recebe intervalo; handler orquestra consulta ao TabSettings

### H. Relatório avançado (RF-054)
- [x] Confirmado: `GET /reports/advanced` é preparatório com feature gate (`features.reports_advanced`); CA-190/191 (CMV, itens mais vendidos, desperdício) são escopo futuro — sem payload real ainda
- [x] Decisão registrada em `02-pendencias-e-futuro.md` (vide seção Relatórios)

### I. Inconsistências de documentação
- [x] Checklist item 20 corrigido: `docs/dominio/03-requisitos-e-aceite.md` existe (CA-182/183 confirmam abertura automática de Tab no arrival)
- [x] `02-pendencias-e-futuro.md` atualizado: `ReservationArrivedEventHandler` é informativo (não no-op); abertura automática implementada em `RegisterArrivalCommandHandler`
- [x] Docs de infraestrutura atualizados com `PlatformSupplierRepository` e `SupplierCategoryRepository`

---

## 22. Correções Pós-Auditoria e PostgreSQL

### A. Migrar EF Core de SQL Server para PostgreSQL
- [x] Remover pacote `Microsoft.EntityFrameworkCore.SqlServer` do `Mangefy.Infrastructure.csproj`
- [x] Adicionar pacote `Npgsql.EntityFrameworkCore.PostgreSQL` (versão 8.x compatível)
- [x] Trocar `UseSqlServer` por `UseNpgsql` em `DependencyInjection.cs`
- [x] Trocar `UseSqlServer` por `UseNpgsql` em `MangefyDbContextFactory.cs`
- [x] Atualizar `appsettings.Development.json` com string de conexão PostgreSQL
- [x] Atualizar docs para referências PostgreSQL (remover LocalDB/SQL Server)

### B. Corrigir sintaxe do índice filtrado para PostgreSQL
- [x] `TabConfiguration`: trocar `"[Status] = 'Open'"` (SQL Server) por `"\"Status\" = 'Open'"` (PostgreSQL)
- [x] Confirmar compatibilidade com CA-104, CA-105, CA-113

### C. Gerar migrations PostgreSQL
- [x] Remover migrations SQL Server existentes (incompatíveis com PostgreSQL)
- [x] Gerar migration `InitialPostgres` cobrindo todo o modelo atual
- [x] Atualizar `MangefyDbContextModelSnapshot`
- [x] Verificar que migration inclui: `ActivationTokens`, `ReservationSettings`, `AuditLogs`, `FiscalDocuments`, `IdempotencyEntries`, `Devices`, `OperationalSessions`, `PrintJobs`, `CashSupplies`, `CashMethodBalances`, `MenuItemPriceHistory`, índices, RowVersion equivalente, campos novos em `Tabs`/`TabPayments`/`OrderItems`/`MenuItems`/`TabSettings`
- [x] Aplicar ao banco: bloqueador externo — requer instância PostgreSQL com credenciais configuradas (`dotnet ef database update`)

### D. Tratar violação de índice único no UnitOfWork
- [x] Capturar `DbUpdateException` com inner `PostgresException` (SqlState `23505`)
- [x] Converter para `ConflictException` (HTTP 409)
- [x] Adicionar using para `Npgsql`
- [x] Garantir que colisão no índice de tabs abertas retorne 409, não 500

### E. Validar status da reserva ANTES de abrir Tab no RegisterArrival
- [x] Mover validação de status da reserva para antes do `ISender.Send(OpenTabCommand)`
- [x] Garantir que reserva `Cancelled`, `NoShow` ou já `Arrived` não crie Tab órfã

### F. Definir comportamento de arrival sem mesa (CA-179)
- [x] CA-179 permite reserva sem mesa; `OpenTabCommand` exige `TableId` ou `LocationNote`
- [x] Definir: usar `LocationNote = "Reserva"` como fallback quando `TableId` for nulo
- [x] Implementar fallback no `RegisterArrivalCommandHandler`
- [x] Decisão documentada: `LocationNote = "Reserva"` quando `TableId` for nulo

### G. Corrigir documentação contraditória restante
- [x] Seção "Notas" do checklist corrigida: `ReservationArrivedEventHandler` é informativo (não no-op)
- [x] Item 3 corrigido: handler `ReservationArrivedEvent` é informativo; abertura automática está em `RegisterArrivalCommandHandler`
- [x] Item 20 corrigido: `docs/dominio/03-requisitos-e-aceite.md` existe (CA-182/183 confirmados)
- [x] Item 9 corrigido: login requer `TenantSlug + email + password`; `GetByEmailInTenantAsync` usado
- [x] `02-camada-infraestrutura.md` atualizado: `GetByEmailInTenantAsync` é usado no login
- [x] Referências a LocalDB/SQL Server removidas dos docs de implementação

---

## 23. Correções Finais de Documentação e Migration

### A. Corrigir `docs/implementacao/02-camada-infraestrutura.md`
- [x] Remover referência a `Microsoft.EntityFrameworkCore.SqlServer` na lista de pacotes
- [x] Substituir por `Npgsql.EntityFrameworkCore.PostgreSQL 8.0.11`
- [x] Corrigir tabela de handlers: `ReservationArrivedEventHandler` é informativo, não `(no-op)`
- [x] Documentar que abertura automática da Tab acontece em `RegisterArrivalCommandHandler`, não no handler de evento
- [x] Remover lista de migrations antigas (`Initial`, `AddWorkforceSettings`, `AddReservationSettings`, `AddActivationTokens`, `AddRobustnessFeatures`) — substituída por `InitialPostgres`

### B. Corrigir `docs/dominio/01-estrutura-e-agregados.md`
- [x] Corrigir seção 14 (Reservas): remover texto `RegisterArrival(tabId)` como input externo
- [x] Documentar fluxo correto: API recebe `EmployeeId`; handler valida status; abre Tab com `OpenTabCommand`; usa `LocationNote = "Reserva"` se `TableId` for nulo; depois vincula `TabId` via `Reservation.RegisterArrival(tabId)` internamente
- [x] Esclarecer que `ReservationArrivedEventHandler` é informativo (para notificações futuras)

### C. Corrigir `docs/dominio/04-diagramas.md`
- [x] Diagrama de estados: substituir `RegisterArrival(tabId)` por `RegisterArrival(employeeId)`
- [x] Remover implicação de que `tabId` é fornecido pelo cliente externo — quem abre a Tab é o handler

### D. Corrigir comentário em `TabConfiguration.cs`
- [x] Linha do comentário antes do `HasIndex`: remover menção a "SQL Server"
- [x] Comentar corretamente: filtro PostgreSQL `WHERE "Status" = 'Open'`, já incluído na migration `InitialPostgres`

### E. Revisar `docs/decisoes/02-pendencias-e-futuro.md`
- [x] Confirmar que migrations estão descritas como `InitialPostgres` gerada e pendente apenas de aplicar
- [x] Confirmar ausência de referências a SQL Server/LocalDB
- [x] Confirmar que fluxo de reservas não menciona `RegisterArrival(tabId)` como input externo

### F. Aplicar migration PostgreSQL
- [ ] Rodar `dotnet ef database update --project Mangefy.Infrastructure --startup-project Mangefy.API`
  - **Resultado:** falhou — `Npgsql.NpgsqlException: Failed to connect to 127.0.0.1:5432` / `SocketException (10061): Nenhuma conexão pôde ser feita porque a máquina de destino as recusou ativamente.`
  - **Motivo:** PostgreSQL não está rodando localmente. Build OK; o comando só falha na tentativa de conexão.
  - **Ação necessária:** iniciar PostgreSQL (Docker, serviço local ou instância remota) e configurar credenciais via user secrets antes de rodar novamente.

---

## Validação Final

- [x] `dotnet build Mangefy.slnx` com zero erros — **Compilação com êxito. 0 Aviso(s), 0 Erro(s)**
- [x] `dotnet test Mangefy.slnx --no-build` — sem saída (nenhum projeto de teste; confirmado)
- [x] Build pós-PostgreSQL: `dotnet build Mangefy.slnx` — **Compilação com êxito. 0 Aviso(s), 0 Erro(s)**
- [x] Migration `InitialPostgres` gerada — `dotnet ef migrations add InitialPostgres` executado com sucesso
- [x] `dotnet build Mangefy.slnx` pós-seção 23 — **Compilação com êxito. 0 Aviso(s), 0 Erro(s)**
- [x] `dotnet test Mangefy.slnx --no-build` pós-seção 23 — sem saída (nenhum projeto de teste; confirmado)

---

## Notas sobre Itens Fora do Escopo desta Rodada

- E-mail real de ativação: futuro
- SignalR / KDS em tempo real: futuro
- Hub fiscal NFC-e real: futuro
- Maquininha / PIX dinâmico: futuro
- iFood / Rappi / Google Meu Negócio: futuro
- Middleware global de bloqueio por turno: pendente (documentado em `02-pendencias-e-futuro.md`)
- Relatórios avançados (RF-054): endpoint criado com feature gate; payload completo é futuro
- `ReservationArrivedEventHandler`: handler **informativo** — abertura automática implementada em `RegisterArrivalCommandHandler`
- Migrations PostgreSQL: requerem banco configurado (ver seção 22-C)

---

## Arquivos Modificados/Criados nesta Rodada

### Domain
- `Mangefy.Domain/Common/Entity.cs` — adicionados `SetCreatedByEmployee` e `SetUpdatedByEmployee`

### Application
- `Mangefy.Application/Tenants/Commands/CreateTenant/CreateTenantCommandHandler.cs` — adicionado `WorkforceSettings` no onboarding; namespace `Stock` e `WorkforceSettings` fully-qualified
- `Mangefy.Application/Platform/Plans/**` — Commands + Queries + Handlers (CreatePlan, UpdatePlan, ActivatePlan, DeactivatePlan, GetPlans)
- `Mangefy.Application/Platform/BusinessTypes/**` — Commands + Queries + Handlers (CreateBusinessType, UpdateBusinessType, AddRoleTemplate, UpdateRoleTemplate, ToggleRoleTemplate, GetBusinessTypes)
- `Mangefy.Application/Platform/PlanFeatureSets/**` — Commands + Queries + Handlers (UpsertPlanFeatureSet, GetPlanFeatureSets)
- `Mangefy.Application/Platform/SupplierCategories/**` — Commands + Queries + Handlers
- `Mangefy.Application/Platform/PlatformSuppliers/**` — Commands + Queries + Handlers

### Infrastructure
- `Mangefy.Infrastructure/Persistence/UnitOfWork.cs` — adicionado `ICurrentUser` + `ApplyAuditInfo()` para rastreabilidade automática
- `Mangefy.Infrastructure/Persistence/Repositories/TabRepository.cs` — corrigido `ToHashSetAsync` → `ToListAsync().ToHashSet()`

### API
- `Mangefy.API/Controllers/TabsController.cs` — permissão de cancelar comanda corrigida para `tabs.cancel`; `CloseTabRequest` com `DiscountReason`; `SubmitOrderItemRequest` ajustado
- `Mangefy.API/Controllers/ReportsController.cs` — endpoint `GET /reports/advanced` com feature gate
- `Mangefy.API/Controllers/Admin/PlansController.cs` — novo (AdminSaas)
- `Mangefy.API/Controllers/Admin/BusinessTypesController.cs` — novo (AdminSaas, inclui RoleTemplates)
- `Mangefy.API/Controllers/Admin/PlanFeatureSetsController.cs` — novo (AdminSaas)
- `Mangefy.API/Controllers/Admin/SupplierCategoriesController.cs` — novo (AdminSaas)
- `Mangefy.API/Controllers/Admin/PlatformSuppliersController.cs` — novo (AdminSaas)

### Build Fixes
- `Mangefy.Application/DailyCash/Queries/GetCurrentCashRegister/CashRegisterDto.cs` — `Difference?.Amount` → `Difference`
- `Mangefy.Application/Tabs/Commands/SubmitOrder/SubmitOrderCommandValidator.cs` — removidas regras `ItemName`/`UnitPrice`
