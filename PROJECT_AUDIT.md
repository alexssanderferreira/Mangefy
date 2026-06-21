# PROJECT_AUDIT.md — Auditoria Técnica Mangefy
Data: 2026-06-21

---

## 1. Visão Geral Real do Projeto

O Mangefy é uma plataforma SaaS multi-tenant para gestão de restaurantes/bares, construída com .NET 8 (Clean Architecture + DDD + CQRS via MediatR) no backend e Angular 21 standalone + signals no frontend.

**Estado atual (baseado no código):**
- Backend está substancialmente completo em termos de domain + application + infrastructure + API.
- Frontend está radicalmente incompleto: apenas o painel Admin tem telas funcionais. O shell `/app/**` (Tenant) tem somente um dashboard placeholder vazio.
- O sistema é funcional para operação AdminSaas (gerenciar owners, tenants, planos, assinaturas).
- Nenhuma tela de operação do tenant existe no frontend (menus, mesas, pedidos, caixa, etc.).

**Stack confirmada pelo código:**
- Backend: .NET 8, EF Core com PostgreSQL (Npgsql), MediatR, FluentValidation, BCrypt work factor 12, JWT HS256
- Frontend: Angular 21, standalone components, signals, HttpClient direto (sem NgRx)
- Banco: PostgreSQL com xmin como RowVersion para concorrência otimista
- Email: Resend (ResendEmailSender confirmado)
- Migrations: 7 migrations existentes (de 14/05 a 27/05/2026)

---

## 2. Mapa de Agregados

| Agregado | Domínio | Application | Infrastructure | API | Frontend | Status |
|---|---|---|---|---|---|---|
| Owner | Completo | Completo (CRUD + activate/deactivate/resend) | Config + Repo + Migration | `GET/POST/PUT/PATCH /api/admin/owners` | Lista + Detalhe na area Admin | ✅ Completo |
| Tenant | Completo | Completo (CRUD + status transitions) | Config + Repo | `GET/POST/PUT/PATCH /api/tenants` | Lista + Detalhe + Tabs na área Admin | ✅ Completo |
| Plan | Completo | CRUD + activate/deactivate/delete | Config + Repo | `GET/POST/PUT/PATCH /api/admin/plans` | Tela de planos na área Admin | ✅ Completo |
| BusinessType + RoleTemplate | Completo | CRUD completo incl. role templates | Config + Repo | `GET/POST/PUT/PATCH /api/admin/business-types` | Lista + Detalhe na área Admin | ✅ Completo |
| Subscription + Invoice | Completo | Create + GenerateInvoice + ConfirmPayment + queries | Config + Repo | `POST/GET /api/admin/subscriptions` (Admin) + `/api/tenants/{id}/subscription` (legado) | Tela Assinaturas + Inadimplentes + aba Financeiro no Tenant | ✅ Completo |
| PlanFeatureSet + FeatureGracePeriod | Completo | Upsert + query | Config + Repo | `GET/POST/PATCH /api/admin/plan-feature-sets` | Tela Feature Matrix na área Admin | ✅ Completo |
| PlatformSupplier + SupplierCategory | Completo | CRUD + toggle | Config + Repo | `GET/POST/PUT/PATCH /api/admin/suppliers` + `/api/admin/supplier-categories` | Tela Fornecedores Admin | ✅ Completo |
| Employee | Completo | Create/Update/Deactivate/GrantTemporaryAccess + queries | Config + Repo | `GET/POST/PUT/PATCH /api/tenants/{id}/employees` | Sem tela no shell /app; apenas visualização na aba de Tenant no Admin | ⚠️ Parcial |
| TenantRole | Completo | Create + UpdatePermissions + GetAll | Config + Repo | `GET/POST/PUT /api/tenants/{id}/roles` | Sem tela | 🔴 Só backend |
| Tab + Order + OrderItem | Completo (domínio rico) | 8 commands + 2 queries (open, submit, close, cancel, KDS status) | Config + Repo | `GET/POST/PATCH /api/tenants/{id}/tabs` | Sem tela | 🔴 Só backend |
| Menu + Category + MenuItem + Recipe | Completo | 12 commands + 2 queries | Config + Repo | `GET/POST/PUT/DELETE/PATCH /api/tenants/{id}/menus` | Sem tela | 🔴 Só backend |
| Table | Completo | Create/Update/SetStatus + GetAll | Config + Repo | `GET/POST/PUT/PATCH /api/tenants/{id}/tables` | Sem tela | 🔴 Só backend |
| Stock + StockItem + StockMovement | Parcial (domínio completo, sem Supplier tenant-level handlers) | 3 commands + 1 query | Config + Repo | `GET/POST/PATCH /api/tenants/{id}/stock` | Sem tela | ⚠️ Parcial |
| Supplier (tenant-level) | Completo no domínio | Sem handlers | Repo existe | Sem controller | Sem tela | 🔴 Só domínio |
| CashRegister + CashWithdrawal | Completo | Open/Close/Withdrawal + 2 queries | Config + Repo | `GET/POST /api/tenants/{id}/cash-registers` | Sem tela | 🔴 Só backend |
| Reservation | Completo | 5 commands + 1 query | Config + Repo | `GET/POST/PATCH /api/tenants/{id}/reservations` | Sem tela | 🔴 Só backend |
| Settings (7 types) | Completo | Get+Update para cada (Payment, Fiscal, Printer, Tab, Reservation, BusinessSchedule, EmployeeSchedule, Workforce) | Config + Repos | `GET/PUT /api/tenants/{id}/settings/*` | Sem tela | 🔴 Só backend |
| Device | Completo | Register/Deactivate + GetAll | Config + Repo | `GET/POST/PATCH /api/tenants/{id}/devices` | Sem tela | 🔴 Só backend |
| OperationalSession | Completo | Start/End + GetActive | Config + Repo | `GET/POST /api/tenants/{id}/operational-sessions` | Sem tela | 🔴 Só backend |
| PrintJob | Completo | MarkAsPrinted + Reprint + GetPending | Config + Repo | `GET/POST/PATCH /api/tenants/{id}/print-jobs` | Sem tela | 🔴 Só backend |
| FiscalDocument | Parcial (domínio stub) | GetFiscalDocuments + CancelFiscalDocument | Config + Repo | `GET/PATCH /api/tenants/{id}/fiscal-documents` | Sem tela | ⚠️ Parcial |
| AuditLog | Completo | GetAuditLogs query | Config + Repo + AuditService | `GET /api/tenants/{id}/audit-logs` | Sem tela | 🔴 Só backend |
| BusinessSchedule | Completo | Get/Update | Config + Repo | Via SettingsController | Sem tela | 🔴 Só backend |
| EmployeeSchedule | Completo | Get/Update | Config + Repo | Via SettingsController | Sem tela | 🔴 Só backend |
| Reports | Parcial | GetSalesReport + GetOperationalReport (+ advanced stub) | Sem repository próprio (lê de Tabs/etc.) | `GET /api/tenants/{id}/reports/sales|operational|advanced` | Sem tela | ⚠️ Parcial |

---

## 3. Mapa de Endpoints

### Auth (`/api/auth`)

| Método | Rota | Handler | Status |
|---|---|---|---|
| POST | `/api/auth/login` | `LoginCommandHandler` | ✅ |
| POST | `/api/auth/resolve-tenants` | `ResolveTenantsCommandHandler` | ✅ |
| POST | `/api/auth/switch-tenant` | `SwitchTenantCommandHandler` | ✅ |
| POST | `/api/auth/set-password` | `SetPasswordCommandHandler` | ✅ |
| POST | `/api/auth/owner/activate` | `ActivateOwnerAccountCommandHandler` | ✅ |
| POST | `/api/auth/admin/login` | `AdminSaasLoginCommandHandler` | ✅ |

### Admin (`/api/admin/*`) — RequireAdminSaas

| Método | Rota | Handler |
|---|---|---|
| GET/POST/PUT/PATCH | `/api/admin/owners` | CRUD + activate/deactivate |
| GET/POST/PUT/PATCH | `/api/tenants` (RequireAdminSaas) | CRUD + status transitions |
| GET/POST/PUT/PATCH | `/api/admin/plans` | CRUD + activate/deactivate |
| GET/POST/PUT/PATCH | `/api/admin/business-types` | CRUD + role templates |
| GET/POST/PATCH | `/api/admin/subscriptions` | Create + invoices + confirm payment + list + overdue |
| GET/POST/PUT/PATCH | `/api/admin/suppliers` + `/api/admin/supplier-categories` | CRUD |
| GET/POST/PATCH | `/api/admin/plan-feature-sets` | Upsert + query |
| GET | `/api/admin/tenants/{id}/employees` | AdminTenantsController |

**Anomalia:** `TenantsController` (CRUD de tenant) está na rota `/api/tenants` com `[RequireAdminSaas]`, mas o nome e path sugerem ser recurso tenant, não admin. `Admin/SubscriptionsController` está em `/api/admin/subscriptions`, enquanto `SubscriptionsController` (legado) está em `/api/tenants/{tenantId}/subscription`. São handlers diferentes (namespaces diferentes: `Mangefy.Application.Subscriptions` vs `Mangefy.Application.Platform.Subscriptions`).

### Tenant (`/api/tenants/{tenantId:guid}/*`) — ValidateTenantAccess + RequirePermission

| Recurso | Rota | Operações |
|---|---|---|
| Tabs | `/tabs` | GET (open), GET/:id, POST (open), POST/:id/orders, POST/:id/close, POST/:id/cancel, PATCH KDS |
| Menus | `/menus` | GET all, GET/:id, POST, PATCH activate/deactivate, POST/PUT/DELETE categories, POST/PUT/DELETE/PATCH items, PUT/DELETE recipe |
| Tables | `/tables` | GET, POST, PUT/:id, PATCH/:id/status |
| Employees | `/employees` | GET, GET/:id, GET/active, POST, PUT/:id, PATCH/:id/deactivate, POST/:id/grant-access |
| Roles | `/roles` | GET, POST, PUT/:id/permissions |
| Stock | `/stock` | GET, POST items, PATCH items/:id/adjust, POST items/:id/purchase |
| CashRegisters | `/cash-registers` | GET current, GET history, POST open, POST close, POST withdrawal |
| Reservations | `/reservations` | GET (by date), POST, PATCH confirm/cancel/no-show/arrival |
| Settings | `/settings/*` | GET/PUT payment, fiscal, tab, reservations, schedule, printers, employees/:id/schedule |
| Workforce | `/settings/workforce` | GET, PUT |
| Devices | `/devices` | GET, POST, PATCH/:id/deactivate |
| OperationalSessions | `/operational-sessions` | GET active, POST start, POST/:id/end |
| PrintJobs | `/print-jobs` | GET pending, POST reprint, PATCH/:id/printed |
| FiscalDocuments | `/fiscal-documents` | GET, PATCH/:id/cancel |
| Reports | `/reports` | GET sales, GET operational, GET advanced |
| AuditLogs | `/audit-logs` | GET |
| Subscription (legado) | `/subscription` | POST, POST invoices, POST invoices/:id/confirm |

---

## 4. Mapa de Telas Angular

### Auth (`/auth`)

| Rota Angular | Componente | Service | Status |
|---|---|---|---|
| `/auth/login` | `LoginComponent` | `AuthService` | ✅ Funcional |
| `/auth/select-tenant` | `SelectTenantComponent` | `AuthService` | ✅ Funcional |
| `/auth/activate-owner` | `ActivateOwnerComponent` | `AuthService` | ✅ Funcional |
| `/plataforma-mgf-console` | `AdminLoginComponent` | `AuthService` | ✅ Funcional |

### Admin (`/admin`)

| Rota Angular | Componente | Service | Status |
|---|---|---|---|
| `/admin/dashboard` | `DashboardComponent` | — | ✅ Existe |
| `/admin/tenants` | `TenantListComponent` | `TenantService` | ✅ Funcional |
| `/admin/tenants/:id` | `TenantDetailComponent` | `TenantService`, `PlansService`, `SubscriptionService` | ✅ Funcional (info + financeiro + employees) |
| `/admin/plans` | `PlansComponent` | `PlansService` | ✅ Funcional |
| `/admin/business-types` | `BusinessTypeListComponent` | `BusinessTypeService` | ✅ Funcional |
| `/admin/business-types/:id` | `BusinessTypeDetailComponent` | `BusinessTypeService` | ✅ Funcional |
| `/admin/owners` | `OwnerListComponent` | `OwnerService` | ✅ Funcional |
| `/admin/owners/:id` | `OwnerDetailComponent` | `OwnerService` | ✅ Funcional |
| `/admin/suppliers` | `SuppliersComponent` | `SupplierService` | ✅ Existe |
| `/admin/suppliers/:id` | `SupplierDetailComponent` | `SupplierService` | ✅ Existe |
| `/admin/subscriptions` | `SubscriptionsComponent` | `SubscriptionService` | ✅ Funcional |
| `/admin/overdue` | `OverdueComponent` | `SubscriptionService` | ✅ Funcional |
| `/admin/feature-matrix` | `FeatureMatrixComponent` | `FeatureMatrixService` | ✅ Existe |

### App / Tenant (`/app`) — QUASE TUDO AUSENTE

| Rota Angular | Componente | Status |
|---|---|---|
| `/app/dashboard` | `AppDashboardComponent` | ⚠️ Placeholder — exibe apenas "Painel do estabelecimento — em breve." |
| `/app/tabs` | — | ❌ Não existe |
| `/app/menus` | — | ❌ Não existe |
| `/app/tables` | — | ❌ Não existe |
| `/app/employees` | — | ❌ Não existe |
| `/app/roles` | — | ❌ Não existe |
| `/app/stock` | — | ❌ Não existe |
| `/app/cash-register` | — | ❌ Não existe |
| `/app/reservations` | — | ❌ Não existe |
| `/app/reports` | — | ❌ Não existe |
| `/app/settings` | — | ❌ Não existe |
| `/app/kds` | — | ❌ Não existe (KDS é backend-ready) |

---

## 5. Inconsistências Entre Camadas

1. **Dois SubscriptionsControllers com handlers diferentes:** `Controllers/SubscriptionsController.cs` (rota `/api/tenants/{id}/subscription`) usa `Mangefy.Application.Subscriptions.*` (namespace antigo), enquanto `Controllers/Admin/SubscriptionsController.cs` (rota `/api/admin/subscriptions`) usa `Mangefy.Application.Platform.Subscriptions.*`. Existem duas implementações paralelas de CreateSubscription, GenerateInvoice e ConfirmPayment em namespaces distintos. O tenant-level é marcado como `[RequireAdminSaas]` — portanto, acessível apenas ao AdminSaas de qualquer forma, o que torna a separação de rota confusa.

2. **Supplier (tenant-level) sem handlers de Application:** O domínio tem `Supplier.cs`, `ISupplierRepository.cs`, `SupplierRepository.cs` e `SupplierConfiguration.cs`, mas não existe nenhum Command/Query handler no Application para criação, edição ou listagem de fornecedores no nível do tenant. O endpoint de `StockController` aceita `SupplierId` ao adicionar item de estoque, mas não há como criar/listar fornecedores do tenant pela API.

3. **AuditInfo incompleta para Owner:** `UnitOfWork.ApplyAuditInfo()` só persiste `EmployeeId`. Quando o AdminSaas ou um Owner realiza ações (login, criar tenant, etc.), `CreatedByEmployeeId` fica null. O campo existe na entidade mas não é preenchido corretamente para esses atores.

4. ~~**RoleTemplate não aplicado na criação de Tenant**~~ — **CORRIGIDO (2026-06-21).** `CreateTenantCommandHandler` agora cria automaticamente: (1) o `TenantRole` do dono (`IsOwnerRole = true`) e (2) um `TenantRole` para cada `RoleTemplate` ativo do `BusinessType` selecionado. `TenantRole.CreateOwnerRole()` foi adicionado ao domínio. Projeto de testes `Mangefy.Application.Tests` criado com 6 testes cobrindo esses cenários.

5. **SubmitOrderRequest usa campos que não chegam ao handler:** O record `SubmitOrderItemRequest` no controller contém `ItemName`, `UnitPrice` e `RequiresKds` como campos explícitos, mas o `SubmitOrderCommand` interno busca essas informações do `MenuItem` no repositório. Há descompasso entre o que o cliente manda e o que o handler realmente usa — o cliente poderia mentir sobre nome/preço.

6. **SwitchTenant para Employee pressupõe mesmo email em todos os tenants:** `SwitchAsEmployee` busca funcionário pelo email no tenant destino. Se o mesmo email não existir no tenant destino, retorna 403. Isso funciona para o caso de Owner (verificação direta por `OwnerId`), mas para Employee multi-tenant (funcionário registrado em vários estabelecimentos), o email precisa ser idêntico em todos.

7. **OperationalSessionsController.Start não requer permissão:** `[HttpPost("start")]` não tem `[RequirePermission]`. Qualquer usuário autenticado do tenant pode iniciar uma sessão operacional. Os outros endpoints de operações (Tabs, Stock, etc.) têm permissão granular, mas a sessão não tem proteção.

8. **Reports/Advanced retorna objeto vazio com código 200:** `GET /api/tenants/{id}/reports/advanced` existe no controller, verifica o feature gate, mas sempre retorna `{ Message = "Relatório avançado em desenvolvimento." }`. Isso pode confundir integrações que esperam dados reais.

9. **Frontend chama `/api/tenants/{id}` diretamente no TenantDetailComponent (sem prefixo /admin):** O `saveEdit()` do `TenantDetailComponent` faz `PUT /api/tenants/{id}` — essa rota existe e funciona, pois `TenantsController` está no caminho `/api/tenants`. Porém, `saveBusinessType()` faz `PATCH /api/tenants/{id}/business-type` e os outros métodos de status usam `TenantService`. Consistência informal, mas sem bug crítico.

10. ~~**Verificação de limites do plano incompleta**~~ — **CORRIGIDO (2026-06-21).** `MaxTables` enforced em `CreateTableCommandHandler`, `MaxMenuItems` em `AddMenuItemCommandHandler`, `MaxUsers` em `CreateEmployeeCommandHandler`. Todos seguem o padrão: busca tenant → busca plan via `tenant.PlanId` → conta registros existentes → lança `ConflictException` se limite atingido.

11. **Domain event `OrderSubmittedEvent` existe no domínio mas sem handler:** `OrderSubmittedEvent.cs` está no domínio, mas não existe `OrderSubmittedEventHandler` no Application. O evento é disparado mas não tem consumidor.

12. **`CashRegisterOpenedEvent` e `CashRegisterClosedEvent` sem handlers:** Esses dois domain events existem no domínio mas não têm handlers no Application.

13. **`StockLowEvent` sem handler:** Existe no domínio, sem handler correspondente.

14. **`TenantCancelledEvent` e `TenantSuspendedEvent` sem handlers:** O domínio levanta esses eventos ao cancelar/suspender tenant, mas não há handlers para encadear efeitos (ex: suspender acesso de employees, enviar notificações).

---

## 6. Riscos Identificados

### Segurança

1. **Vazamento cross-tenant via ausência de filtro TenantId em queries:** Vários handlers de query simplesmente buscam por ID sem verificar `TenantId`. Ex: se `GetTabByIdQuery(tenantId, tabId)` verifica o `TenantId` corretamente no repositório, está ok — mas é um ponto sensível que precisa ser auditado handler por handler. O `ValidateTenantAccessAttribute` protege a rota, mas queries que buscam por ID direto (sem filtrar por tenant) podem ser exploradas se o ID for descoberto.

2. **`ResolveTenantsCommand` expõe existência de email:** Retorna 403 com "Credenciais inválidas" para qualquer falha, o que é correto. Mas a resposta inclui lista de tenants do owner — atacante com credenciais válidas obtém mapa completo dos estabelecimentos.

3. **Nenhum rate limiting:** Nem o login, nem o resolve-tenants têm rate limiting configurado em `Program.cs`. Tentativas de força-bruta não são bloqueadas.

4. **JWT sem refresh token:** O token expira (configurável via `Jwt:ExpirationMinutes`, default 480 min = 8h). Não há endpoint de refresh. Após expirar, o usuário precisa refazer login completo. O frontend tem `isSessionValid()` que verifica a data, mas não há renovação automática.

5. **AdminSaas bypass total no ValidateTenantAccess:** `IsAdminSaas = true` bypassa completamente a verificação de `TenantId`. Se um token AdminSaas for comprometido, o atacante tem acesso irrestrito a todos os tenants em todos os endpoints.

6. **Token armazenado em localStorage (frontend):** `mgf_token` e `mgf_user` ficam em `localStorage`. Vulnerável a XSS. Sem flag HttpOnly por ser SPA, mas é uma decisão consciente. Se houver XSS no Angular, o token é extraível.

### Performance

7. **SubmitOrderCommand provavelmente carrega o Tab inteiro:** O padrão DDD requer carregar o aggregate completo para modificá-lo. Comandos que crescem (Tab com muitos pedidos) causam carregamento de objetos grandes. Sem paginação em `GetOpenTabsQuery`.

8. **AuditLog sem paginação:** `GetAuditLogsQuery(tenantId, from, to)` pode retornar volumes enormes se o range de datas for grande. Não há paginação.

9. **ForkJoin no TenantDetailComponent carrega planos e business types a cada abertura:** `ngOnInit` dispara 3 chamadas paralelas (tenant + planos + business types). Não há cache — cada navegação à tela recarrega tudo.

### Integridade

10. ~~**Ausência de enforcement de limites de plano**~~ — **CORRIGIDO (2026-06-21).** Todos os quatro limites agora são enforced nos handlers de criação.

11. **TenantPlanChangedEventHandler existe mas o que faz?** O handler existe mas não foi auditado em profundidade. Se não desativar roles/features corretamente em downgrade, o tenant mantém capacidades além do plano.

---

## 7. Próximos Passos

### P1 — Crítico para MVP do Tenant

1. **Construir o shell `/app/**`:** Estrutura de rotas, shell com sidebar/topbar para o funcionário logado. Sem isso, o backend está 100% sem interface de uso real.

2. **Tela de Mesas (`/app/tables`):** Visualização das mesas com status (Livre/Ocupada), criação e edição. Dependência direta do fluxo de Tab.

3. **Tela de Commandas/Tabs (`/app/tabs`):** Abertura de comanda, visualização de comandas abertas, submissão de pedido, fechamento com pagamento. É o núcleo operacional do sistema.

4. ~~**Implementar verificação de limites de plano nos handlers**~~ — **CONCLUÍDO (2026-06-21).** `CreateTable`, `AddMenuItem` e `CreateEmployee` agora verificam `MaxTables`, `MaxMenuItems` e `MaxUsers`. `CreateRole` já estava correto.

5. ~~**Corrigir criação automática de TenantRoles na criação de Tenant**~~ — **CONCLUÍDO (2026-06-21).**

### P2 — Operações Essenciais

6. **Tela de Cardápio (`/app/menus`):** Criar/editar menus, categorias, itens. Necessário para que Tab possa aceitar pedidos.

7. **Tela de Funcionários (`/app/employees`):** Listagem, criação, edição, desativação e atribuição de roles.

8. **Tela de Roles (`/app/roles`):** Criação de cargos personalizados com seleção de permissões.

9. **Handlers de Supplier (tenant-level):** Criar `Application/Suppliers/` com commands/queries para CRUD de fornecedores do tenant e conectar ao `StockController`.

10. **KDS (Kitchen Display System):** Tela para a cozinha visualizar pedidos em preparação. Backend está pronto (endpoints de `start/ready/return` no TabsController).

### P3 — Funcionalidades Adicionais

11. **Tela de Caixa (`/app/cash-register`):** Abertura/fechamento de caixa, registro de retiradas.

12. **Tela de Reservas (`/app/reservations`):** Calendário de reservas, confirmação, registro de chegada.

13. **Tela de Estoque (`/app/stock`):** Visualização, adição de itens, ajuste de inventário, registro de compra.

14. **Tela de Relatórios (`/app/reports`):** Sales report + operational report com filtros de data.

15. **Tela de Configurações (`/app/settings`):** Painel de configurações do tenant (horários, pagamento, impressoras, etc.).

16. **Rate limiting no backend:** Adicionar `AspNetCoreRateLimit` ou similar para login e resolve-tenants.

17. **Handlers para eventos de domínio sem consumidor:** OrderSubmittedEvent, CashRegisterOpenedEvent, CashRegisterClosedEvent, StockLowEvent, TenantCancelledEvent, TenantSuspendedEvent.

---

## 8. Dívidas Técnicas

### Backend

1. **Duplicação de SubscriptionsController:** Existe `Controllers/SubscriptionsController.cs` (rota `/api/tenants/{id}/subscription`, handler namespace antigo `Mangefy.Application.Subscriptions`) E `Controllers/Admin/SubscriptionsController.cs` (rota `/api/admin/subscriptions`, handler namespace `Mangefy.Application.Platform.Subscriptions`). São dois sets de handlers para o mesmo aggregate. O tenant-level é `RequireAdminSaas`, então não faz sentido existir separado do admin. Deve ser consolidado.

2. **Supplier tenant-level modelado mas não implementado acima do repositório:** Domínio + infra existem, Application + API ausentes. O `StockController.AddItem` aceita `SupplierId` mas não há como criar fornecedores pelo tenant.

3. ~~**Limites de plano parcialmente enforced**~~ — **CORRIGIDO (2026-06-21).** Todos os quatro limites enforced: `MaxCustomRoles` (`CreateRole`), `MaxTables` (`CreateTable`), `MaxMenuItems` (`AddMenuItem`), `MaxUsers` (`CreateEmployee`).

4. **AuditInfo não captura ações de Owner/AdminSaas:** `UnitOfWork.ApplyAuditInfo()` só seta `EmployeeId`. Quando Owner ou AdminSaas executa ações, `CreatedByEmployeeId`/`UpdatedByEmployeeId` ficam null, perdendo rastreabilidade.

5. **`ReportController.GetAdvanced` é endpoint fictício:** Existe, verifica feature gate, mas retorna estrutura vazia com mensagem "em desenvolvimento". Deveria ser marcado como 501 Not Implemented ou removido.

6. **`OrderSubmittedEvent`, `CashRegisterOpenedEvent`, `CashRegisterClosedEvent`, `StockLowEvent`, `TenantCancelledEvent`, `TenantSuspendedEvent` sem handlers:** 6 domain events levantados sem consumidor no Application. Efeitos colaterais importantes (notificação ao cliente, bloqueio de acesso, alertas de estoque) não acontecem.

7. **`StartSession` sem `[RequirePermission]`:** Qualquer usuário autenticado do tenant pode criar sessão operacional.

8. **`IntegrationSettings` sem controller/handler:** Existe no domínio, infra e repositório, mas não há endpoint para get/update de configurações de integração.

### Frontend

9. **`/app/**` está completamente vazio:** O aplicativo do tenant existe somente como dashboard placeholder. Todo o backend está construído sem interface.

10. **Sem mecanismo de refresh de token:** Token expira após 8h sem renovação automática. O `isSessionValid()` checa expiração mas não dispara refresh. O `authGuard` eventualmente bloqueará o usuário sem aviso prévio durante uso ativo.

11. **`TenantDetailComponent` acessa API diretamente via `HttpClient` injetado, sem service:** Para salvar tenant (`saveEdit`) e algumas ações específicas (`saveBusinessType`, `lookupCep`), o componente injeta `HttpClient` diretamente em vez de usar `TenantService`. Viola o padrão de serviços adotado pelo restante do frontend.

12. **Nenhum interceptor para tratamento global de erros 401/403:** O `auth.interceptor.ts` provavelmente só adiciona o Bearer token. Não há tratamento padronizado para expiração de sessão ou erros de autorização que redirecionem para login ou exibam mensagem.

13. **Sem tela de criação de Tenant no Admin:** O backend tem `POST /api/tenants` para criar tenant, mas o frontend Admin não tem tela de criação de novo estabelecimento. A lista de tenants existe, mas o AdminSaas não consegue criar um tenant pela interface.

14. **Sem tela de criação de Owner no Admin que gere Tenant simultaneamente:** O fluxo natural (criar Owner → criar Tenant → criar Subscription) está separado em telas distintas e sem wizard guiado.
