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
│   └── SubscriptionsController.cs
├── Filters/
│   ├── RequirePermissionAttribute.cs    ← IAsyncActionFilter — verifica permissão via ICurrentUser
│   └── RequireAdminSaasAttribute.cs     ← IAsyncActionFilter — verifica IsAdminSaas
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
| POST | `/api/auth/set-password` | AllowAnonymous | Define senha no primeiro acesso |
| POST | `/api/auth/admin/login` | AllowAnonymous | Login do AdminSaas — retorna JWT com claim `isAdminSaas=true` |

---

### TenantsController — `api/tenants`

Toda a classe requer `[RequireAdminSaas]`.

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/tenants` | Lista todos os tenants |
| GET | `/api/tenants/{id}` | Retorna tenant por ID |
| POST | `/api/tenants` | Cria novo tenant |
| PUT | `/api/tenants/{id}` | Atualiza nome, timezone, slug |
| PATCH | `/api/tenants/{id}/plan` | Troca plano do tenant |
| PATCH | `/api/tenants/{id}/suspend` | Suspende ou reativa o tenant |

---

### EmployeesController — `api/tenants/{tenantId}/employees`

| Método | Rota | Permissão | Descrição |
|--------|------|-----------|-----------|
| GET | `/employees` | `employees.read` | Lista todos os employees do tenant |
| GET | `/employees/{id}` | `employees.read` | Retorna employee por ID |
| GET | `/employees/active` | `employees.read` | Lista employees com turno ativo agora |
| POST | `/employees` | `employees.manage` | Cria employee |
| PUT | `/employees/{id}` | `employees.manage` | Atualiza nome e cargo |
| PATCH | `/employees/{id}/deactivate` | `employees.manage` | Desativa employee |
| POST | `/employees/{id}/grant-access` | `employees.manage` | Estende acesso temporário |

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
| PATCH | `/tabs/{id}/orders/{orderId}/items/{itemId}/ready` | `orders.update_status` | KDS marca item como pronto |

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
| POST | `/cash-registers/close` | `cash.manage` | Fecha caixa |
| POST | `/cash-registers/withdrawal` | `cash.manage` | Registra sangria |

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
| PUT | `/settings/workforce` | `settings.manage` | Atualiza tolerância de turno em minutos |

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
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MangefyDb;Trusted_Connection=True;"
  },
  "Jwt": {
    "Key": "super-secret-key-at-least-32-chars-long",
    "Issuer": "Mangefy",
    "Audience": "Mangefy",
    "ExpirationMinutes": "480"
  },
  "AdminSaas": {
    "Email": "admin@mangefy.com",
    "PasswordHash": "$2a$12$..."
  }
}
```

> **Segurança:** As chaves `Jwt:Key` e `AdminSaas:PasswordHash` nunca devem ser comitadas em repositório. Em produção, usar variáveis de ambiente ou Azure Key Vault.

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

---

## Pendências da Camada API

- [ ] Aplicar migration `AddReservationSettings` no banco
- [ ] SignalR Hub para KDS e notificações ao garçom (próxima etapa)
- [ ] Testes de integração dos endpoints principais
