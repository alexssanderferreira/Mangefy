# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

---

## Project Overview

**Mangefy** is a SaaS multi-tenant restaurant management platform. Each restaurant/bar is a **Tenant** (isolated dataset). The platform is operated by **AdminSaas** (Mangefy company).

Two security realms:
- **Platform** (`/admin/**`): AdminSaas manages plans, business types, subscriptions, owners.
- **Tenant** (`/app/**`): Restaurant employees operate tabs, menus, stock, cash register, reservations.

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

Pipeline behaviors: `ValidationBehavior` (FluentValidation runs before every handler).

### DDD Aggregates

Key aggregates and their boundaries:

| Aggregate | Responsibility |
|---|---|
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

### Domain Events (raised inside aggregates)

Events are collected on the aggregate and published by the `UnitOfWork` before/after `SaveChangesAsync`. Current active handlers:
- `OrderReadyEvent` → deduct stock via recipe, route to KDS/printer
- `TabOpenedEvent` → mark table Occupied
- `TabClosedEvent` → release table if no open tabs remain
- `TenantPlanChangedEvent` → enforce feature grace periods

### Value Objects

`Email`, `Money`, `Address`, `PhoneNumber` — validated at construction; immutable. Never store a raw string for these.

### Authentication

**Three JWT token flavors:**
1. **Employee token**: `sub=employeeId`, `tenantId`, `permission[]` (multiple claims)
2. **Owner token**: same structure, `IsOwner` role bypasses permission checks
3. **AdminSaas token**: `sub=adminId`, `isAdminSaas=true`, no tenantId

BCrypt work factor 12 for all passwords.

**Authorization attributes:**
- `[Authorize]` — valid JWT required
- `[RequireAdminSaas]` — filter checks `isAdminSaas` claim
- `[ValidateTenantAccess]` — filter checks `tenantId` claim matches route param

### EF Core Notes

- PostgreSQL via Npgsql.
- All entity configs in `Mangefy.Infrastructure/Persistence/Configurations/` (one file per entity).
- **`OwnsOne` / `OwnsMany`**: Value objects mapped with `OwnsOne`, owned collections with `OwnsMany`. EF Core requires the owned entity to be configured carefully — no shared table with another aggregate.
- Domain events: stored in a `List<IDomainEvent>` on `AggregateRoot`; not mapped to DB; cleared after publish.
- Connection string: `ConnectionStrings__DefaultConnection` env var (double underscore = nested section).

### Frontend: Angular 21 Standalone

- All components are `standalone: true`.
- State via **signals** (`signal()`, `computed()`), not NgRx.
- HTTP calls in `*.service.ts` files (one per feature).
- **No shared module** — import pipes/directives directly in `imports[]`.
- Lazy-loaded routes: admin and app shells are separate route groups.
- SignalR used for KDS and live table status.

**Route structure:**
```
/                           → redirect to /auth/login
/auth/**                    → public (login, activate, reset)
/plataforma-mgf-console     → hidden AdminSaas login
/admin/**                   → AdminSaas panel (authGuard + adminGuard)
/app/**                     → Tenant app (authGuard)
```

---

## Key Constraints & Conventions

- **One plan per tenant** (not per owner). Each establishment has its own subscription and billing.
- **Owner has unlimited establishments** — no cap enforced in domain or app layer.
- **Tab is per-person**, never anonymous. Must have a customer name.
- **TenantRole**: one role per employee. Owner role is immutable and always unrestricted.
- **Custom roles** are capped by `Plan.MaxCustomRoles`; roles disabled (not deleted) on downgrade.
- **Feature grace period**: 30 days when a feature is removed from plan matrix.
- **Fiscal (NFC-e)**: fully modeled in domain; API integration is future work.
- **Stock deduction**: triggered by `OrderReadyEvent`, calculated from `RecipeIngredient` snapshot.
- **DomainException** → HTTP 422 (Unprocessable Entity) via `ExceptionHandlingMiddleware`.
- **Money** is always `decimal`, currency stored alongside amount; never use `double` for money.

---

## Docs

| Path | Content |
|---|---|
| `docs/dominio/` | DDD structure, RBAC model, acceptance criteria, diagrams |
| `docs/decisoes/` | Design decisions (Q&A), backlog, product vision, offline plans |
| `docs/implementacao/` | Layer-by-layer implementation notes, API contracts, frontend auth |
| `docs/briefing-frontend-claude.md` | Frontend screen specs and component guidelines |
| `etapas/` | Development milestone tracker |
| `Backend/README.local.md` | Local dev setup (user-secrets, env vars) |
