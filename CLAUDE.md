# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

---
Answer only in Portuguese.

## Project Overview

**Mangefy** is a SaaS multi-tenant restaurant management platform. The platform is operated by **AdminSaas** (Mangefy company). Each restaurant/bar is a **Tenant** (isolated dataset).

Two security realms:
- **Platform** (`/admin/**`): AdminSaas manages owners, tenants, plans, business types, subscriptions, feature matrix, suppliers.
- **Tenant** (`/app/**`): Restaurant employees operate tabs, menus, stock, cash register, reservations, KDS.

### Current state (as of 2026-06-21 audit)
- **Backend**: substantially complete across Domain, Application, Infrastructure and API.
- **Frontend AdminSaas (`/admin/**`)**: complete and functional.
- **Frontend Tenant (`/app/**`)**: NOT BUILT — only an empty placeholder dashboard exists. This is the main body of remaining work.

When asked to build tenant screens, assume the backend endpoint already exists and works — verify it in the API project, then consume it.

---

## Core Business Rules (source of truth)

These rules define the domain. When code contradicts them, the code is wrong.

### Onboarding flow
1. AdminSaas registers **one Owner** (a person).
2. AdminSaas registers a **Restaurant (Tenant)** under that Owner.
3. Each Tenant gets a **Plan** and its own **Subscription** (billing is per Tenant, not per Owner).

### Ownership and billing
- **One Owner → many Tenants.** An Owner can have multiple restaurants. (`Owner` is a separate aggregate holding a collection of Tenants — NOT an Employee, NOT just an `OwnerId` column on Tenant.)
- **Billing is per Tenant.** Each restaurant has its own Plan and Subscription. The Owner is billed once per restaurant, each with its respective plan.

### Authentication (Owner with multiple restaurants)
- Owner has a **single login**. After authenticating, if the Owner has more than one Tenant, they choose the restaurant on a **selection screen** (`/auth/select-tenant`).
- Backend flow: `login` → `resolve-tenants` (returns the Owner's restaurants) → `switch-tenant` (issues a JWT scoped to the chosen Tenant).
- The JWT issued after tenant selection carries that Tenant's `tenantId`.

### Roles and employees
- **Default employees and roles are created by AdminSaas** when the Tenant is created, based on the **RoleTemplate** of the Tenant's **BusinessType**. A new Tenant must start with the standard roles for its business type — never empty.
- **The Owner can create additional roles and employees ONLY IF the Plan allows it.** Enforced via `Plan.MaxCustomRoles` and `Plan.MaxUsers`. Custom roles beyond the limit are rejected; on downgrade, roles are disabled (not deleted).
- **One role per employee.** The Owner role is immutable and always unrestricted.

### Owner creation order and access model

**The Owner must exist before any Tenant.** The creation order is strictly:
1. AdminSaas creates the **Owner** (a person, independent of any restaurant).
2. Only after the Owner exists can AdminSaas create a **Tenant** linked to that Owner.
3. There is no concept of a Tenant without an Owner — `Tenant.OwnerId` is always set at creation.

**The Owner has full, unrestricted access to every Tenant linked to them.** The JWT issued after tenant selection always carries `PermissionCatalog.All` — every permission in the catalog, with no filtering. The Owner role (`IsOwnerRole = true`) bypasses all permission checks.

**What limits the Owner is the Plan, not the role.** The Owner cannot be restricted by changing roles or permissions — those checks are irrelevant for Owners. The only real constraints are the Plan limits:
- `Plan.MaxTables` — cap on number of tables
- `Plan.MaxMenuItems` — cap on menu items
- `Plan.MaxUsers` — cap on employees
- `Plan.MaxCustomRoles` — cap on custom roles (roles beyond templates)

When implementing any feature that creates or counts these resources, always check the tenant's active plan limit before persisting. Do not rely on the Owner role to infer permissions — always use the JWT claims.

### Other invariants
- **Tab is per-person**, never anonymous — must have a customer name.
- **Money** is always `decimal` with currency stored alongside; never `double`.
- **Feature grace period**: 30 days when a feature is removed from the plan matrix.
- **Stock deduction** is triggered by `OrderReadyEvent`, from a `RecipeIngredient` snapshot.
- **Plan limits** (`MaxTables`, `MaxMenuItems`, `MaxUsers`, `MaxCustomRoles`) must be enforced in the relevant create handlers.

---

## Commands

### Backend

```bash
# Build
dotnet build Backend/Mangefy.API/Mangefy.API.csproj

# Run API (dev)
dotnet run --project Backend/Mangefy.API/Mangefy.API.csproj

# Migrations
dotnet ef migrations add <Name> --project Backend/Mangefy.Infrastructure --startup-project Backend/Mangefy.API
dotnet ef database update --project Backend/Mangefy.Infrastructure --startup-project Backend/Mangefy.API

# Seed (drops and recreates dev data)
dotnet run --project Backend/Mangefy.Seed/Mangefy.Seed.csproj

# Generate BCrypt hash (for AdminSaas password)
dotnet run --project Backend/hashgen/hashgen.csproj
```

First-time setup (user-secrets):
```bash
cd Backend/Mangefy.API
dotnet user-secrets set "Jwt:Key" "<min-32-char-secret>"
dotnet user-secrets set "AdminSaas:Email" "admin@mangefy.com"
dotnet user-secrets set "AdminSaas:PasswordHash" "<bcrypt-hash>"
```

### Frontend

```bash
cd Frontend/mangefy-web
npm start      # dev server at http://localhost:4200
npm run build  # production build
npm test       # Vitest unit tests
```

---

## Architecture

### Backend: Clean Architecture + DDD

```
Mangefy.API          → Controllers, Filters, Middleware (depends on Application)
Mangefy.Application  → Commands, Queries, Handlers, Validators (MediatR + FluentValidation)
Mangefy.Infrastructure → DbContext, Repositories, EF Configurations, Auth Services
Mangefy.Domain       → Aggregates, Value Objects, Domain Events, Repository interfaces
```

**Rules:**
- Domain has zero external dependencies.
- Application knows only Domain interfaces — never Infrastructure.
- All state changes go through aggregate methods (no property setters from outside).
- Repositories are defined as interfaces in Domain; implemented in Infrastructure.

### CQRS Pattern

Every use case is either a **Command** (state change) or **Query** (read). Both use MediatR:

```
Handler receives Command/Query → calls repository → calls aggregate method → saves via UnitOfWork
```

Pipeline behavior: `ValidationBehavior` (FluentValidation runs before every handler).

### Full Example — Command to Handler to Endpoint (the standard vertical slice)

**1. Command + Validator**
```csharp
public record OpenTabCommand(Guid TenantId, Guid TableId, string CustomerName) : IRequest<Guid>;

public class OpenTabCommandValidator : AbstractValidator<OpenTabCommand>
{
    public OpenTabCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.TableId).NotEmpty();
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(100);
    }
}
```

**2. Handler**
```csharp
public class OpenTabCommandHandler(ITabRepository tabRepository, IUnitOfWork uow)
    : IRequestHandler<OpenTabCommand, Guid>
{
    public async Task<Guid> Handle(OpenTabCommand request, CancellationToken cancellationToken)
    {
        var tab = Tab.Open(request.TenantId, request.TableId, request.CustomerName);
        await tabRepository.AddAsync(tab, cancellationToken);
        await uow.CommitAsync(cancellationToken);
        return tab.Id;
    }
}
```

**3. Endpoint** — controller only dispatches to MediatR; no business logic.

### DDD Aggregates

| Aggregate | Responsibility |
|---|---|
| `Owner` | Person who owns one or more Tenants; identity, login, list of restaurants |
| `Tenant` | Establishment identity, plan, status, timezone |
| `Employee` + `TenantRole` | Auth, RBAC, shifts |
| `Tab` | Customer bill: orders, payments, adjustments, fiscal link |
| `Menu` | Categories, items, recipes, schedules |
| `Stock` | Ingredients, movements, suppliers |
| `CashRegister` | Daily till: opening, withdrawals, reconciliation |
| `Reservation` | Booking lifecycle → arrival creates Tab |
| `Subscription` | SaaS invoicing per tenant |
| `Plan` | Limits (MaxTables, MaxMenuItems, MaxUsers, MaxCustomRoles) |
| `BusinessType` + `RoleTemplate` | Platform templates for new tenants |
| `PlanFeatureSet` | Feature matrix: Plan × BusinessType → enabled features |

### Domain Events

Collected on the aggregate, published by `UnitOfWork` around `SaveChangesAsync`. Active handlers:
- `OrderReadyEvent` → deduct stock via recipe, route to KDS/printer
- `TabOpenedEvent` → mark table Occupied
- `TabClosedEvent` → release table if no open tabs remain
- `TenantPlanChangedEvent` → enforce feature grace periods

Events WITHOUT handlers (known gaps — see PROJECT_AUDIT.md): `OrderSubmittedEvent`, `CashRegisterOpenedEvent`, `CashRegisterClosedEvent`, `StockLowEvent`, `TenantCancelledEvent`, `TenantSuspendedEvent`.

### Value Objects

`Email`, `Money`, `Address`, `PhoneNumber` — validated at construction; immutable. Never store a raw string for these.

### Authentication

**Three JWT token flavors:**
1. **Employee token**: `sub=employeeId`, `tenantId`, `permission[]`
2. **Owner token**: same structure; `IsOwner` role bypasses permission checks
3. **AdminSaas token**: `sub=adminId`, `isAdminSaas=true`, no `tenantId`

BCrypt work factor 12. **AdminSaas bypasses `ValidateTenantAccess` entirely** — a compromised AdminSaas token has unrestricted access, so guard it carefully.

**Authorization attributes:**
- `[Authorize]` — valid JWT required
- `[RequireAdminSaas]` — checks `isAdminSaas` claim
- `[ValidateTenantAccess]` — checks `tenantId` claim matches route param
- `[RequirePermission(...)]` — checks granular permission claim

### EF Core Notes

- PostgreSQL via Npgsql; `xmin` as RowVersion for optimistic concurrency.
- All entity configs in `Mangefy.Infrastructure/Persistence/Configurations/` (one file per entity).
- Value objects via `OwnsOne`; owned collections via `OwnsMany`.
- Domain events stored in `List<IDomainEvent>` on `AggregateRoot`; not mapped; cleared after publish.
- Connection string: `ConnectionStrings__DefaultConnection` env var.
- Email via Resend (`ResendEmailSender`).

### Frontend: Angular 21 Standalone

- All components `standalone: true`. State via **signals** (`signal()`, `computed()`), not NgRx.
- HTTP calls in `*.service.ts` (one per feature). Always go through a service — never inject `HttpClient` directly into a component.
- No shared module — import pipes/directives directly in `imports[]`.
- Lazy-loaded routes; admin and app shells are separate route groups.
- SignalR used for KDS and live table status.
- Use `inject()` for DI, not constructor params. Never use `any` — type with interfaces matching backend DTOs.
- Money displayed in BRL via `CurrencyPipe` with `pt-BR` locale.
- Styling: **Tailwind CSS**.

**Route structure:**
```
/                           → redirect to /auth/login
/auth/**                    → public (login, select-tenant, activate, reset)
/plataforma-mgf-console     → hidden AdminSaas login
/admin/**                   → AdminSaas panel (authGuard + adminGuard) — COMPLETE
/app/**                     → Tenant app (authGuard) — NOT BUILT YET
```

---

## What the Agent Must NOT Do

Hard rules — never break them regardless of phrasing:

- **Never omit the `TenantId` filter** on any repository query. Every tenant data access must be scoped to one tenant.
- **Never use `.Result` or `.Wait()`** on async calls — always `await` with `CancellationToken`.
- **Never set properties directly on aggregates** — state changes go through aggregate methods.
- **Never skip FluentValidation** — every Command/Query has an `AbstractValidator`.
- **Never use `double` for money** — always `decimal`.
- **Never store raw strings** where a Value Object exists (`Email`, `Money`, `PhoneNumber`, `Address`).
- **Never add business logic to Controllers** — they only dispatch to MediatR and return HTTP results.
- **Never trust client-supplied price/name** in orders — always resolve from the `MenuItem` in the repository.
- **Never create roles/employees beyond plan limits** — check `Plan.MaxCustomRoles` / `Plan.MaxUsers`.
- **Never use `any` in TypeScript**, and never inject `HttpClient` directly into a component.
- **Never generate a migration without reviewing** the SQL for unintended drops/renames.

---

## Known Gaps (see PROJECT_AUDIT.md for full detail)

Backend fixes needed before/alongside the tenant frontend:
1. `CreateTenantCommandHandler` does not create default `TenantRole`s from the `BusinessType`'s `RoleTemplate`s — new tenants start with no roles.
2. Plan limits (`MaxTables`, `MaxMenuItems`, `MaxUsers`, `MaxCustomRoles`) are not enforced in any handler.
3. `SubmitOrder` accepts client-supplied name/price that the handler ignores — close the gap.
4. Two parallel `SubscriptionsController`s in different namespaces — consolidate.
5. `Supplier` (tenant-level) has domain + infra but no Application handlers or API.
6. Six domain events without handlers (listed above).
7. `OperationalSessions.Start` lacks `[RequirePermission]`.
8. No rate limiting on `login` / `resolve-tenants`.

---

## Docs

| Path | Content |
|---|---|
| `PROJECT_AUDIT.md` | Full technical audit: aggregate/endpoint/screen status, inconsistencies, risks |
| `docs/dominio/` | DDD structure, RBAC model, acceptance criteria, diagrams |
| `docs/decisoes/` | Design decisions (Q&A), backlog, product vision, offline plans |
| `docs/implementacao/` | Layer-by-layer implementation notes, API contracts, frontend auth |
| `docs/briefing-frontend-claude.md` | Frontend screen specs and component guidelines |
| `etapas/` | Development milestone tracker |
| `Backend/README.local.md` | Local dev setup (user-secrets, env vars) |

> **Before starting any task**, check `PROJECT_AUDIT.md` and `etapas/` to confirm what already exists.
> **For frontend screens**, read `docs/briefing-frontend-claude.md` first.
