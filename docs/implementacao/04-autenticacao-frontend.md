# Autenticação Frontend — Mangefy

## Visão geral

O frontend tem dois mundos de acesso completamente separados, com identidade visual distinta para evitar confusão operacional e dificultar tentativas de acesso não autorizado ao painel administrativo.

---

## Acesso do Tenant (Estabelecimento)

**Rota:** `/auth/login`  
**Tema visual:** preto + amarelo (`#f5c400`)  
**Redirect após login:** `/app/dashboard`

### Fluxo
1. Funcionário acede `/auth/login`
2. Submete e-mail + senha → `POST /api/auth/login`
3. API retorna JWT com `employeeId`, `tenantId`, `permissions[]`
4. Token e dados do utilizador são persistidos em `localStorage`
5. `AuthService` expõe `user` (signal), `loggedIn` (computed), `hasPermission()`
6. Guard `authGuard` protege todas as rotas `/app/**`

### Componente
`src/app/features/auth/login/login.component.ts`

---

## Acesso Admin SaaS (Plataforma)

**Rota:** `/plataforma-mgf-console` ← URL secreta, não referenciada em nenhuma tela  
**Tema visual:** preto-avermelhado + vermelho (`#e03131`)  
**Redirect após login:** `/admin/dashboard`

### Decisão de segurança
O painel Admin SaaS não tem link visível em nenhuma parte da aplicação. A URL é conhecida apenas pela equipa interna da Mangefy. Qualquer URL desconhecida redireciona para `/auth/login`, sem revelar a existência do painel admin.

### Fluxo
1. Operador acede directamente à URL secreta
2. Submete credenciais → `POST /api/auth/admin/login`
3. JWT retornado inclui `isAdmin: true`
4. Guard `adminGuard` verifica `isAdmin` antes de permitir acesso a `/admin/**`
5. Tentativa de acesso a `/admin/**` sem `isAdmin` redireciona para `/plataforma-mgf-console`

### Componente
`src/app/features/auth/admin-login/admin-login.component.ts`

---

## Estrutura de rotas

```
/                          → redirect /auth/login
/auth/login                → LoginComponent          (tenant)
/plataforma-mgf-console    → AdminLoginComponent     (admin — URL secreta)
/admin/**                  → canActivate: [authGuard, adminGuard]
/app/**                    → canActivate: [authGuard]
/**                        → redirect /auth/login    (sem pistas sobre rotas admin)
```

---

## AuthService

`src/app/core/auth/auth.service.ts`

| Membro | Tipo | Descrição |
|--------|------|-----------|
| `user` | `Signal<CurrentUser \| null>` | Utilizador autenticado |
| `loggedIn` | `Signal<boolean>` | Computed de `user !== null` |
| `isAdmin` | `Signal<boolean>` | Computed de `user.isAdmin` |
| `login(req)` | `Observable` | Login do tenant |
| `adminLogin(req)` | `Observable` | Login admin SaaS |
| `logout()` | `void` | Limpa storage e redireciona para `/auth/login` |
| `hasPermission(p)` | `boolean` | Verifica permissão RBAC do cargo |

Persistência: `localStorage` com chaves `mgf_token` e `mgf_user`.

---

## Interceptor JWT

`src/app/core/interceptors/auth.interceptor.ts`

Adiciona automaticamente `Authorization: Bearer <token>` em todos os pedidos HTTP. Configurado em `app.config.ts` via `withInterceptors`.

---

## Guards

`src/app/core/guards/auth.guard.ts`

| Guard | Condição | Redirect |
|-------|----------|----------|
| `authGuard` | `loggedIn()` | `/auth/login` |
| `adminGuard` | `isAdmin()` | `/plataforma-mgf-console` |
