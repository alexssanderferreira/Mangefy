# Frontend — Painel Admin SaaS

Stack: Angular 18+, standalone components, signals, `@if`/`@for` control flow.

---

## Estrutura de arquivos

```
src/app/
├── core/
│   ├── auth/auth.service.ts          — signals: user, loggedIn, isAdmin
│   ├── guards/auth.guard.ts          — authGuard + adminGuard
│   ├── interceptors/auth.interceptor.ts — Bearer token automático
│   └── models/auth.models.ts
├── features/
│   ├── auth/
│   │   ├── login/                    — login tenant (preto + amarelo)
│   │   ├── admin-login/              — login admin (preto + vermelho)
│   │   └── auth.routes.ts
│   ├── admin/
│   │   ├── shell/                    — AdminShellComponent + sidebar + topbar
│   │   ├── dashboard/                — placeholder
│   │   ├── plans/                    — ✅ implementado
│   │   ├── tenants/tenant-list/      — placeholder
│   │   └── admin.routes.ts
│   └── app/
│       └── app.routes.ts             — rotas do painel do estabelecimento (vazio)
└── environments/environment.ts
```

---

## Padrões adotados

- **Sem NgModules** — todos os componentes são `standalone: true`
- **Signals** para estado local (`signal`, `computed`)
- **Inline template + styles** — sem arquivos separados `.html`/`.scss` por componente
- **Drawer lateral** para criar/editar (slide-in da direita, 420px)
- **Toast** no canto inferior direito para feedback pós-ação (3,5s, fade-out automático)
- **Modal** centralizado com overlay para ações destrutivas (exclusão)
- **Skeleton cards** no loading state

---

## Telas implementadas — Admin

### ✅ Login Admin (`/plataforma-mgf-console`)
URL secreta, não referenciada em nenhuma tela. Visual preto + vermelho.

### ✅ Planos (`/admin/plans`)

**Componentes:** `plans.component.ts`, `plans.service.ts`

**Funcionalidades:**
- Lista separada em seções **Ativos** (topo) e **Inativos** (abaixo)
- Cards com: nome, descrição, preço, limites, badge de status
- Botões por card:
  - Ativo: Editar / Desativar / Excluir
  - Inativo: Editar / Ativar / Excluir
- Drawer lateral para Criar e Editar
- Modal de confirmação para exclusão
- Toast de sucesso após salvar ou excluir

**Campos do formulário:**
- Nome (só na criação — imutável após criar)
- Descrição (editável em criação e edição)
- Preço mensal — auto-formatação BRL em tempo real (centavos à direita)
- Máx. mesas, itens, utilizadores, cargos — só dígitos inteiros (keydown bloqueado)

**Serviço (`PlansService`):**
```
GET    /api/admin/plans             — lista todos (ativos + inativos)
POST   /api/admin/plans             — criar
PUT    /api/admin/plans/{id}        — editar (preço, limites, descrição)
PATCH  /api/admin/plans/{id}/activate
PATCH  /api/admin/plans/{id}/deactivate
DELETE /api/admin/plans/{id}
```

**Backend alterado nesta sessão:**
- `IPlanRepository` — adicionados `GetAllAsync` e `DeleteAsync`
- `PlanRepository` — implementações dos dois métodos acima
- `GetPlansQueryHandler` — usa `GetAllAsync` (retorna todos, não só ativos)
- `Plan.UpdateDescription(string?)` — novo método de domínio
- `UpdatePlanCommand/Handler` — inclui `Description`
- `DELETE /api/admin/plans/{id}` — novo endpoint
- `UpdatePlanRequest` — inclui `Description`

---

## Telas pendentes — Admin

| Tela | Prioridade | Observação |
|------|-----------|------------|
| **Tenants** (lista + detalhe + criar) | Alta | Próxima sugerida — fluxo core do SaaS |
| **Dashboard** | Alta | Depende de dados de assinatura |
| **Assinaturas** | Média | Faturas, confirmar pagamento |
| **Tipos de Negócio** | Média | CRUD + templates de cargo |
| **Matriz de Features** | Média | Tabela plano × tipo de negócio |
| **Fornecedores / Categorias** | Baixa | CRUD simples |

---

## Variáveis CSS globais usadas

```css
--color-brand: /* vermelho admin */
```

Definidas no `styles.scss` global.
