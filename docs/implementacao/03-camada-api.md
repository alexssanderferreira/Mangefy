# Camada de API — Mangefy

Documentação da camada API: controllers, middleware, configuração do host.

---

## Visão Geral

A camada API é o ponto de entrada HTTP do sistema. Expõe os casos de uso da Application via controllers REST, configura autenticação JWT, Swagger e tratamento centralizado de exceções.

**Projeto:** `Mangefy.API`  
**Pacotes principais:**
- Microsoft.AspNetCore.Authentication.JwtBearer 8.0.11
- Swashbuckle.AspNetCore (Swagger)

---

## Estrutura de Pastas

```
Mangefy.API/
├── Controllers/
│   ├── AuthController.cs
│   ├── TenantsController.cs
│   ├── EmployeesController.cs
│   ├── RolesController.cs
│   ├── MenusController.cs
│   ├── TablesController.cs
│   ├── TabsController.cs
│   ├── ReservationsController.cs
│   ├── CashRegistersController.cs
│   ├── StockController.cs
│   ├── SettingsController.cs
│   ├── WorkforceSettingsController.cs
│   ├── SubscriptionsController.cs
│   ├── AuditLogsController.cs
│   ├── ReportsController.cs             ← GET /sales, /operational, /advanced (feature gate)
│   ├── FiscalDocumentsController.cs
│   ├── DevicesController.cs
│   ├── OperationalSessionsController.cs
│   ├── PrintJobsController.cs
│   └── Admin/
│       ├── PlansController.cs           ← [RequireAdminSaas] CRUD de planos
│       ├── BusinessTypesController.cs   ← [RequireAdminSaas] CRUD de tipos de negócio + role templates
│       ├── PlanFeatureSetsController.cs ← [RequireAdminSaas] matriz Plano × Tipo de Negócio
│       ├── SupplierCategoriesController.cs ← [RequireAdminSaas] categorias globais de fornecedor
│       └── PlatformSuppliersController.cs  ← [RequireAdminSaas] catálogo global de fornecedores
├── Filters/
│   ├── RequirePermissionAttribute.cs    ← IAsyncActionFilter — verifica permissão via ICurrentUser
│   ├── RequireAdminSaasAttribute.cs     ← IAsyncActionFilter — verifica IsAdminSaas
│   └── ValidateTenantAccessAttribute.cs ← IAsyncActionFilter — verifica que TenantId do token == rota
├── Middleware/
│   └── ExceptionHandlingMiddleware.cs
├── Services/
│   └── HttpContextCurrentUser.cs        ← implementa ICurrentUser lendo claims do HttpContext
├── Program.cs
└── appsettings.json
```

---

## Controllers e Endpoints

Todos os controllers seguem o padrão:
- Herdam `ControllerBase`
- Recebem `ISender` (MediatR) via construtor
- Delegam diretamente ao MediatR — sem lógica de negócio
- `[ApiController]` garante model binding automático

---

### AuthController — `api/auth`

| Método | Rota | Autorização | Descrição |
|--------|------|-------------|-----------|
| POST | `/api/auth/login` | AllowAnonymous | Login de employee — retorna JWT com tenantId e permissões |
| POST | `/api/auth/set-password` | AllowAnonymous | Define senha via token de ativação (`Token` + `NewPassword`) |
| POST | `/api/auth/admin/login` | AllowAnonymous | Login do AdminSaas — retorna JWT com claim `isAdminSaas=true` |

> **set-password:** O body agora é `{ "token": "<guid32chars>", "newPassword": "..." }`. O token é obtido na resposta de criação do employee ou do tenant. Tokens expiram em 48h e são inválidos após o primeiro uso.

---

### TenantsController — `api/tenants`

Toda a classe requer `[RequireAdminSaas]`.

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/tenants` | Lista todos os tenants |
| GET | `/api/tenants/{id}` | Retorna tenant por ID |
| POST | `/api/tenants` | Cria novo tenant |
| PUT | `/api/tenants/{id}` | Atualiza nome, timezone, slug |
| PATCH | `/api/tenants/{id}/plan` | Troca plano do tenant — 409 se o plano alvo estiver inativo |
| PATCH | `/api/tenants/{id}/business-type` | Troca tipo de negócio do tenant — body: `{ "businessTypeId": "guid" }` |
| PATCH | `/api/tenants/{id}/suspend` | Suspende ou reativa o tenant |
| PATCH | `/api/tenants/{id}/reactivate` | Reativa o tenant |
| PATCH | `/api/tenants/{id}/cancel` | Cancela o tenant |

---

### EmployeesController — `api/tenants/{tenantId}/employees`

| Método | Rota | Permissão | Descrição |
|--------|------|-----------|-----------|
| GET | `/employees` | `employees.read` | Lista todos os employees do tenant |
| GET | `/employees/{id}` | `employees.read` | Retorna employee por ID |
| GET | `/employees/active` | `employees.read` | Lista employees com turno ativo agora |
| POST | `/employees` | `employees.manage` | Cria employee — body: `{ name, email, tenantRoleId }` — retorna `{ employeeId, activationToken }` |
| PUT | `/employees/{id}` | `employees.manage` | Atualiza nome e cargo |
| PATCH | `/employees/{id}/deactivate` | `employees.manage` | Desativa employee |
| POST | `/employees/{id}/grant-access` | `employees.manage` | Estende acesso temporário |

> **Criação de employee:** O campo `passwordHash` foi removido do request. A API retorna um `activationToken` que deve ser usado no endpoint `set-password` para o funcionário definir sua senha. Em produção, esse token seria enviado por e-mail (pendente de implementação).

---

### RolesController — `api/tenants/{tenantId}/roles`

| Método | Rota | Permissão | Descrição |
|--------|------|-----------|-----------|
| GET | `/roles` | `roles.read` | Lista todos os cargos do tenant |
| POST | `/roles` | `roles.manage` | Cria novo cargo |
| PUT | `/roles/{id}/permissions` | `roles.manage` | Atualiza permissões do cargo |

---

### MenusController — `api/tenants/{tenantId}/menus`

| Método | Rota | Permissão | Descrição |
|--------|------|-----------|-----------|
| GET | `/menus` | `menu.read` | Lista todos os cardápios com hierarquia completa |
| GET | `/menus/{id}` | `menu.read` | Retorna cardápio específico |
| POST | `/menus` | `menu.manage` | Cria cardápio |
| PATCH | `/menus/{id}/activate` | `menu.manage` | Ativa cardápio |
| PATCH | `/menus/{id}/deactivate` | `menu.manage` | Desativa cardápio |
| POST | `/menus/{id}/categories` | `menu.manage` | Adiciona categoria |
| PUT | `/menus/{id}/categories/{catId}` | `menu.manage` | Atualiza categoria |
| DELETE | `/menus/{id}/categories/{catId}` | `menu.manage` | Remove categoria |
| POST | `/menus/{id}/categories/{catId}/items` | `menu.manage` | Adiciona item |
| PUT | `/menus/{id}/categories/{catId}/items/{itemId}` | `menu.manage` | Atualiza item |
| DELETE | `/menus/{id}/categories/{catId}/items/{itemId}` | `menu.manage` | Remove item |
| PATCH | `/menus/{id}/categories/{catId}/items/{itemId}/status` | `menu.manage` | Altera status do item |
| PUT | `/menus/{id}/categories/{catId}/items/{itemId}/recipe` | `menu.manage` | Define ficha técnica (requer `features.stock_basic`) |
| DELETE | `/menus/{id}/categories/{catId}/items/{itemId}/recipe` | `menu.manage` | Remove ficha técnica |

**Request — SetRecipe:**
```json
{
  "ingredients": [
    { "stockItemId": "guid", "stockItemName": "Farinha", "quantity": 0.3, "unit": "kg" }
  ]
}
```

---

### TablesController — `api/tenants/{tenantId}/tables`

| Método | Rota | Permissão | Descrição |
|--------|------|-----------|-----------|
| GET | `/tables` | `tables.read` | Lista todas as mesas |
| POST | `/tables` | `tables.manage` | Cria mesa |
| PUT | `/tables/{id}` | `tables.manage` | Atualiza número, capacidade, seção |
| PATCH | `/tables/{id}/status` | `tables.manage` | Altera status (Available / Unavailable) |

---

### TabsController — `api/tenants/{tenantId}/tabs`

| Método | Rota | Permissão | Descrição |
|--------|------|-----------|-----------|
| GET | `/tabs` | `tabs.read` | Lista todas as comandas abertas |
| GET | `/tabs/{id}` | `tabs.read` | Retorna comanda específica |
| POST | `/tabs` | `tabs.create` | Abre comanda |
| POST | `/tabs/{id}/orders` | `orders.create` | Envia pedido |
| POST | `/tabs/{id}/close` | `tabs.close` | Fecha comanda com pagamentos |
| POST | `/tabs/{id}/cancel` | `tabs.close` | Cancela comanda com motivo |
| PATCH | `/tabs/{id}/orders/{orderId}/items/{itemId}/start` | `orders.update_status` | KDS inicia preparo |
| PATCH | `/tabs/{id}/orders/{orderId}/items/{itemId}/ready` | `orders.update_status` | KDS marca item como pronto (requer `features.kds`) |
| PATCH | `/tabs/{id}/orders/{orderId}/items/{itemId}/return` | `orders.update_status` | Devolve item ao fluxo de preparo |
| PATCH | `/tabs/{id}/orders/{orderId}/items/{itemId}/cancel` | `orders.update_status` | Cancela item com motivo opcional |

**Request — OpenTab (atualizado):**
```json
{
  "employeeId": "guid",
  "customerName": "João",
  "tableId": null,
  "locationNote": "Balcão 3",
  "channel": "InPerson",
  "deliveryInfo": null,
  "clientCommandId": "guid-opcional"
}
```

`channel` aceita: `InPerson` | `Delivery` | `TakeAway`. Se `Delivery`, o campo `deliveryInfo` é obrigatório:
```json
{
  "recipientName": "João",
  "address": "Rua X, 100",
  "complement": "Apto 5",
  "phoneNumber": "11999999999",
  "externalOrderRef": "REF-IFOOD-123"
}
```

**Request — SubmitOrder (atualizado):**
```json
{
  "employeeId": "guid",
  "items": [
    {
      "menuItemId": "guid",
      "itemName": "X-Burguer",
      "unitPrice": 28.90,
      "quantity": 2,
      "requiresKds": true,
      "notes": "sem cebola",
      "modifiers": ["extra queijo", "sem tomate"]
    }
  ],
  "clientCommandId": "guid-opcional"
}
```

**Request — CloseTab (atualizado):**
```json
{
  "payments": [
    { "amount": 50.00, "method": "CreditCard", "changeGiven": 0, "externalReference": null },
    { "amount": 10.00, "method": "Pix", "changeGiven": 0, "externalReference": "pix-tx-id" }
  ],
  "discountAmount": 5.00,
  "serviceFee": 6.00,
  "tip": 3.00
}
```

**Request — CancelTab:**
```json
{ "reason": "Pedido duplicado" }
```

**Request — CancelOrderItem:**
```json
{ "reason": "Cliente desistiu" }
```
_(reason é opcional — pode enviar `{}`)_

---

### ReservationsController — `api/tenants/{tenantId}/reservations`

| Método | Rota | Permissão | Descrição |
|--------|------|-----------|-----------|
| GET | `/reservations?date=yyyy-MM-dd` | `reservations.read` | Lista reservas de uma data |
| POST | `/reservations` | `reservations.manage` | Cria reserva |
| PATCH | `/reservations/{id}/confirm` | `reservations.manage` | Confirma reserva |
| PATCH | `/reservations/{id}/cancel` | `reservations.manage` | Cancela reserva com motivo |
| PATCH | `/reservations/{id}/no-show` | `reservations.manage` | Marca como No-Show |
| PATCH | `/reservations/{id}/arrival` | `reservations.manage` | Registra chegada → abre Tab |

---

### CashRegistersController — `api/tenants/{tenantId}/cash-registers`

| Método | Rota | Permissão | Descrição |
|--------|------|-----------|-----------|
| GET | `/cash-registers/current` | `cash.manage` | Retorna caixa aberto atual |
| GET | `/cash-registers/history?from=&to=` | `cash.manage` | Histórico de caixas por período |
| POST | `/cash-registers/open` | `cash.manage` | Abre caixa |
| POST | `/cash-registers/close` | `cash.manage` | Fecha caixa com contagem por método |
| POST | `/cash-registers/withdrawal` | `cash.manage` | Registra sangria |

**Request — CloseCashRegister (atualizado):**
```json
{
  "methodBalances": [
    { "method": "Cash", "expectedAmount": 350.00, "countedAmount": 348.50 },
    { "method": "Pix",  "expectedAmount": 120.00, "countedAmount": 120.00 }
  ],
  "employeeId": "guid",
  "notes": "Diferença de R$1,50 no dinheiro — possível troco incorreto"
}
```
`notes` é obrigatório quando qualquer método tiver `|countedAmount - expectedAmount| > 0,01`.

---

### StockController — `api/tenants/{tenantId}/stock`

| Método | Rota | Permissão | Descrição |
|--------|------|-----------|-----------|
| GET | `/stock` | `stock.read` | Lista todos os itens de estoque |
| POST | `/stock/items` | `stock.manage` | Adiciona item ao estoque |
| PATCH | `/stock/items/{id}/adjust` | `stock.manage` | Ajuste manual de quantidade |
| POST | `/stock/items/{id}/purchase` | `stock.manage` | Registra entrada de compra |

---

### SettingsController — `api/tenants/{tenantId}/settings`

Todos os endpoints requerem `settings.manage`.

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/settings/payment` | Retorna métodos de pagamento habilitados |
| PUT | `/settings/payment` | Atualiza métodos habilitados |
| GET | `/settings/fiscal` | Retorna configurações fiscais (sem retornar a chave da API) |
| PUT | `/settings/fiscal` | Habilita/desabilita NF-Ce |
| GET | `/settings/printers` | Lista impressoras |
| POST | `/settings/printers` | Adiciona impressora |
| PUT | `/settings/printers/{id}` | Atualiza impressora |
| DELETE | `/settings/printers/{id}` | Remove impressora |
| GET | `/settings/tab` | Retorna intervalo de números de comanda |
| PUT | `/settings/tab` | Atualiza intervalo de números de comanda |
| GET | `/settings/schedule` | Retorna grade de horários do estabelecimento |
| PUT | `/settings/schedule` | Atualiza grade, dias especiais e política de fechamento |
| GET | `/settings/reservations` | Retorna limite de reservas simultâneas |
| PUT | `/settings/reservations` | Atualiza limite de reservas simultâneas |
| GET | `/settings/employees/{id}/schedule` | Retorna escala semanal de um funcionário |
| PUT | `/settings/employees/{id}/schedule` | Atualiza escala semanal de um funcionário |

---

### WorkforceSettingsController — `api/tenants/{tenantId}/settings/workforce`

| Método | Rota | Permissão | Descrição |
|--------|------|-----------|-----------|
| GET | `/settings/workforce` | `settings.manage` | Retorna tolerância de turno configurada |
| PUT | `/settings/workforce` | `settings.manage` | Atualiza tolerância de turno em minutos |

---

### AuditLogsController — `api/tenants/{tenantId}/audit-logs`

Requer `[ValidateTenantAccess]`.

| Método | Rota | Permissão | Descrição |
|--------|------|-----------|-----------|
| GET | `/audit-logs?from=&to=` | `settings.manage` | Lista logs de auditoria por período |

**Resposta — AuditLogDto:**
```json
{
  "id": "guid",
  "tenantId": "guid",
  "employeeId": "guid ou null",
  "isAdminSaas": false,
  "action": "tab.cancelled",
  "entityType": "Tab",
  "entityId": "guid",
  "reason": "Pedido errado",
  "before": null,
  "after": null,
  "occurredAt": "2024-01-15T18:30:00Z"
}
```

---

### ReportsController — `api/tenants/{tenantId}/reports`

Requer `[ValidateTenantAccess]`.

| Método | Rota | Permissão | Feature Gate | Descrição |
|--------|------|-----------|--------------|-----------|
| GET | `/reports/sales?from=&to=` | `reports.read` | — | Relatório de vendas |
| GET | `/reports/operational` | `reports.read` | — | Snapshot operacional atual |
| GET | `/reports/advanced?from=&to=` | `reports.read` | `features.reports_advanced` | Analytics avançado (RF-054) — payload preparatório, implementação futura |

**Resposta — SalesReport:**
```json
{
  "from": "2024-01-01",
  "to": "2024-01-31",
  "totalRevenue": 15430.00,
  "totalDiscounts": 320.00,
  "cancellations": 3,
  "byDay": [{ "date": "2024-01-01", "revenue": 500.00, "tabCount": 12 }],
  "topItems": [{ "menuItemId": "guid", "name": "X-Burguer", "quantity": 47, "revenue": 1360.00 }],
  "byPaymentMethod": [{ "method": "CreditCard", "total": 9800.00, "count": 64 }]
}
```

**Resposta — OperationalReport:**
```json
{
  "generatedAt": "2024-01-15T20:00:00Z",
  "openTabs": 8,
  "delayedItems": 2,
  "lowStockItems": [{ "stockItemId": "guid", "name": "Farinha", "currentQuantity": 0.5, "minQuantity": 2.0 }]
}
```

---

### FiscalDocumentsController — `api/tenants/{tenantId}/fiscal-documents`

Requer `[ValidateTenantAccess]`.

| Método | Rota | Permissão | Descrição |
|--------|------|-----------|-----------|
| GET | `/fiscal-documents?from=&to=` | `settings.manage` | Lista documentos fiscais por período |
| PATCH | `/fiscal-documents/{id}/cancel` | `settings.manage` | Cancela documento fiscal com motivo |

**Request — CancelFiscalDocument:**
```json
{ "reason": "NFC-e emitida com erro de valor" }
```

---

### DevicesController — `api/tenants/{tenantId}/devices`

| Método | Rota | Permissão | Descrição |
|--------|------|-----------|-----------|
| GET | `/devices` | `settings.manage` | Lista todos os dispositivos do tenant |
| POST | `/devices` | `settings.manage` | Registra novo dispositivo |
| PATCH | `/devices/{id}/deactivate` | `settings.manage` | Desativa dispositivo |

---

### OperationalSessionsController — `api/tenants/{tenantId}/operational-sessions`

| Método | Rota | Permissão | Descrição |
|--------|------|-----------|-----------|
| GET | `/operational-sessions/active` | `employees.read` | Lista sessões ativas do tenant |
| POST | `/operational-sessions/start` | (autenticado) | Inicia sessão; encerra sessão anterior do mesmo funcionário; verifica turno via `IShiftEnforcementService` |
| POST | `/operational-sessions/{id}/end` | (autenticado) | Encerra sessão |

**Request — StartSession:**
```json
{ "deviceId": "guid-opcional" }
```
**Response:**
```json
{ "id": "guid", "isWithinShift": true }
```

---

### PrintJobsController — `api/tenants/{tenantId}/print-jobs`

| Método | Rota | Permissão | Descrição |
|--------|------|-----------|-----------|
| GET | `/print-jobs/pending` | `settings.manage` | Lista trabalhos de impressão pendentes |
| POST | `/print-jobs/reprint` | `settings.manage` | Cria reimpressão com motivo obrigatório; grava `AuditLog` (`print_job.reprinted`) |
| PATCH | `/print-jobs/{id}/printed` | `settings.manage` | Marca trabalho como impresso |

**Request — Reprint:**
```json
{
  "station": "Kitchen",
  "payload": "{\"tabId\": \"guid\", \"orderId\": \"guid\"}",
  "reason": "Comanda perdida na cozinha",
  "printerId": "guid-opcional"
}
```

---

---

### PlansController — `api/admin/plans` ← AdminSaas

Requer `[RequireAdminSaas]`.

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/admin/plans` | Lista **todos** os planos (ativos e inativos) |
| POST | `/admin/plans` | Cria plano |
| PUT | `/admin/plans/{id}` | Atualiza preço, limites e descrição |
| PATCH | `/admin/plans/{id}/activate` | Ativa plano |
| PATCH | `/admin/plans/{id}/deactivate` | Desativa plano |
| DELETE | `/admin/plans/{id}` | Remove plano permanentemente |

**Body de `PUT /admin/plans/{id}`:** `monthlyPrice`, `maxTables`, `maxMenuItems`, `maxUsers`, `maxCustomRoles`, `description?`

---

### BusinessTypesController — `api/admin/business-types` ← AdminSaas

Requer `[RequireAdminSaas]`. Inclui gerenciamento de Role Templates como sub-recursos.

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/admin/business-types` | Lista todos os tipos com templates, `tenantCount` e `usageCount` por template |
| POST | `/admin/business-types` | Cria tipo de negócio |
| PUT | `/admin/business-types/{id}` | Atualiza nome e descrição |
| PATCH | `/admin/business-types/{id}/activate` | Ativa tipo de negócio |
| PATCH | `/admin/business-types/{id}/deactivate` | Desativa tipo de negócio |
| DELETE | `/admin/business-types/{id}` | Exclui tipo — 422 se tiver templates ou tenants associados |
| POST | `/admin/business-types/{id}/role-templates` | Adiciona template de cargo |
| PUT | `/admin/business-types/{id}/role-templates/{templateId}` | Atualiza template |
| PATCH | `/admin/business-types/{id}/role-templates/{templateId}/activate` | Ativa template |
| PATCH | `/admin/business-types/{id}/role-templates/{templateId}/deactivate` | Desativa template |
| DELETE | `/admin/business-types/{id}/role-templates/{templateId}` | Exclui template — 422 se `usageCount > 0` |

**Body — POST/PUT role-template:** `{ "name": "string", "description": "string?", "permissions": ["orders.read", ...] }`
Permissões são validadas contra `PermissionCatalog` — retorna 422 com lista das inválidas.

---

### PlanFeatureSetsController — `api/admin/plans/{planId}/feature-sets` ← AdminSaas

Requer `[RequireAdminSaas]`. Define quais features estão habilitadas para cada combinação Plano × Tipo de Negócio.

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/admin/plans/{planId}/feature-sets` | Lista feature sets do plano |
| PUT | `/admin/plans/{planId}/feature-sets/{businessTypeId}` | Upsert — define lista completa de features habilitadas |

---

### SupplierCategoriesController — `api/admin/supplier-categories` ← AdminSaas

Requer `[RequireAdminSaas]`. Categorias globais (TenantId = null).

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/admin/supplier-categories` | Lista categorias globais |
| POST | `/admin/supplier-categories` | Cria categoria global |
| PUT | `/admin/supplier-categories/{id}` | Atualiza categoria |

---

### PlatformSuppliersController — `api/admin/platform-suppliers` ← AdminSaas

Requer `[RequireAdminSaas]`. Catálogo global de fornecedores.

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/admin/platform-suppliers?categoryId=` | Lista fornecedores (filtro opcional por categoria) |
| POST | `/admin/platform-suppliers` | Cria fornecedor |
| PUT | `/admin/platform-suppliers/{id}` | Atualiza fornecedor |

---

### SubscriptionsController — `api/tenants/{tenantId}/subscription`

Toda a classe requer `[RequireAdminSaas]`.

| Método | Rota | Descrição |
|--------|------|-----------|
| POST | `/subscription` | Cria assinatura |
| POST | `/subscription/invoices` | Gera fatura |
| PATCH | `/subscription/invoices/{id}/payment` | Confirma pagamento |

---

## Middleware de Exceções

`ExceptionHandlingMiddleware` captura exceções não tratadas e retorna respostas padronizadas:

| Exceção | HTTP Status |
|---------|------------|
| `FluentValidation.ValidationException` | 400 Bad Request |
| `NotFoundException` | 404 Not Found |
| `ForbiddenException` | 403 Forbidden |
| `ConflictException` | 409 Conflict |
| `DomainException` | 422 Unprocessable Entity |
| Outras | 500 Internal Server Error |

Corpo da resposta de erro:
```json
{
  "type": "ValidationError",
  "errors": ["campo X é obrigatório"]
}
```

---

## Program.cs

```csharp
builder.Services.AddApplication();         // MediatR + Validators + Pipeline
builder.Services.AddInfrastructure(config); // DbContext + Repos + Auth Services

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = config["Jwt:Issuer"],
            ValidAudience = config["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(config["Jwt:Key"]!))
        };
    });

// Middleware pipeline (ordem importa):
app.UseExceptionHandlingMiddleware();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

---

## appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "SET_VIA_ENV_OR_USER_SECRETS"
  },
  "Jwt": {
    "Key": "SET_VIA_ENV_OR_USER_SECRETS",
    "Issuer": "mangefy-api",
    "Audience": "mangefy-clients",
    "ExpirationMinutes": "480"
  },
  "AdminSaas": {
    "Email": "SET_VIA_ENV_OR_USER_SECRETS",
    "PasswordHash": "SET_VIA_ENV_OR_USER_SECRETS"
  }
}
```

`appsettings.json` contém apenas placeholders. Valores reais são fornecidos via:
- **Desenvolvimento:** `dotnet user-secrets` — veja `Backend/README.local.md`
- **Produção:** variáveis de ambiente (`Jwt__Key`, `AdminSaas__Email`, etc.)

`appsettings.Development.json` contém a connection string PostgreSQL para desenvolvimento local (usuario/senha via `dotnet user-secrets`).

---

## ICurrentUser

`HttpContextCurrentUser` implementado em `Services/HttpContextCurrentUser.cs`. Lê as claims do `HttpContext.User`:

| Propriedade | Claim JWT |
|-------------|-----------|
| `EmployeeId` | `sub` (`ClaimTypes.NameIdentifier`) |
| `TenantId` | `tenantId` |
| `Permissions` | `permission` (múltiplas claims) |
| `IsAdminSaas` | `isAdminSaas` |

Registrado no DI em `Program.cs`:
```csharp
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpContextCurrentUser>();
```

---

## Filtros de Autorização

### RequirePermissionAttribute
`IAsyncActionFilter` que verifica `ICurrentUser.HasPermission(permission)`. Retorna 403 se a permissão estiver ausente. O Owner sempre passa pois recebe `PermissionCatalog.All` no token.

### RequireAdminSaasAttribute
`IAsyncActionFilter` que verifica `ICurrentUser.IsAdminSaas`. Retorna 403 se falso. Aplicado no nível da classe em `TenantsController` e `SubscriptionsController`.

### ValidateTenantAccessAttribute
`IAsyncActionFilter` que garante isolamento multi-tenant: compara o `tenantId` da URL com `ICurrentUser.TenantId` (extraído do JWT). Retorna 403 se houver divergência ou se `TenantId` for nulo. AdminSaas (`IsAdminSaas=true`) é isento — possui acesso legítimo a qualquer tenant via `RequireAdminSaas`. Aplicado no nível da classe em todos os controllers com rota `api/tenants/{tenantId:guid}/...`:
- `EmployeesController`, `RolesController`, `MenusController`, `TablesController`
- `TabsController`, `ReservationsController`, `CashRegistersController`
- `StockController`, `SettingsController`, `WorkforceSettingsController`

---

## Catálogo de Permissões (`PermissionCatalog`)

Todas as permissões disponíveis na plataforma. Definidas em `Mangefy.Domain/Roles/PermissionCatalog.cs`. O Owner recebe todas no token.

| Permissão | Descrição | Endpoint(s) que usam |
|-----------|-----------|----------------------|
| `orders.read` | Leitura de pedidos | — |
| `orders.create` | Criar pedido (SubmitOrder) | `POST /tabs/{id}/orders` |
| `orders.update_status` | Atualizar status de item (KDS) | `PATCH .../items/{id}/start-preparation`, `.../ready`, `.../return` |
| `orders.cancel` | Cancelar item (Pending) | `PATCH .../items/{id}/cancel` |
| `orders.cancel_after_sent` | Cancelar item Sent/Preparing (+ motivo) | verificado no handler |
| `orders.cancel_in_preparation` | Cancelar item Ready (+ motivo) | verificado no handler |
| `orders.cancel_delivered` | Cancelar item Delivered (+ motivo) | verificado no handler |
| `tabs.read` | Leitura de comandas | — |
| `tabs.create` | Abrir comanda | `POST /tabs` |
| `tabs.close` | Fechar comanda | `POST /tabs/{id}/close` |
| `tabs.cancel` | Cancelar comanda | `POST /tabs/{id}/cancel` |
| `tabs.apply_discount` | Aplicar desconto até `MaxDiscountPercent` | verificado no handler |
| `tabs.apply_discount_override` | Desconto acima do limite | verificado no handler |
| `tabs.apply_courtesy` | Cortesia (comanda gratuita) | futuro |
| `menu.read` | Leitura de cardápios | — |
| `menu.manage` | Gerenciar cardápios/categorias/itens | `MenusController` |
| `tables.read` | Leitura de mesas | — |
| `tables.manage` | Gerenciar mesas | `TablesController` |
| `employees.read` | Leitura de funcionários | `EmployeesController GET` |
| `employees.manage` | Gerenciar funcionários | `EmployeesController POST/PUT/PATCH` |
| `roles.read` | Leitura de cargos | `RolesController GET` |
| `roles.manage` | Gerenciar cargos | `RolesController POST/PUT` |
| `stock.read` | Leitura de estoque | `StockController GET` |
| `stock.manage` | Gerenciar estoque | `StockController POST/PATCH` |
| `cash.manage` | Gerenciar caixa | `CashRegistersController` |
| `reservations.read` | Leitura de reservas | `ReservationsController GET` |
| `reservations.manage` | Gerenciar reservas | `ReservationsController POST/PATCH` |
| `reports.read` | Leitura de relatórios | `ReportsController` |
| `settings.manage` | Gerenciar configurações | `SettingsController`, `WorkforceSettingsController`, `DevicesController`, `PrintJobsController` |

---

## Pendências da Camada API

- [ ] Gerar e aplicar migration `AddRobustnessFeatures` (ver `02-camada-infraestrutura.md` — lista completa)
- [ ] Implementar envio de e-mail com link de ativação ao criar employee/tenant (hoje o token é retornado no response apenas)
- [ ] SignalR Hub para KDS e notificações ao garçom (próxima etapa)
- [ ] **Testes** — ver pendências técnicas em `docs/decisoes/02-pendencias-e-futuro.md`
