# Perfis, Cargos e Permissões — Mangefy

## O modelo de negócio

O Mangefy é uma plataforma **SaaS multi-tenant**. Existem dois mundos distintos:

```
┌──────────────────────────────────────────────────────────────────┐
│                      PLATAFORMA MANGEFY                          │
│               (AdminSaas — dono do sistema)                      │
│                                                                  │
│  ┌──────────────────┐  ┌──────────────────┐  ┌────────────────┐  │
│  │  Restaurante A   │  │  Restaurante B   │  │    Bar C       │  │
│  │  (Tenant A)      │  │  (Tenant B)      │  │  (Tenant C)    │  │
│  │                  │  │                  │  │                │  │
│  │ Plano Basic      │  │ Plano Pro        │  │ Plano Basic    │  │
│  │ Tipo: Restaurante│  │ Tipo: Restaurante│  │ Tipo: Bar      │  │
│  │ Cargos próprios  │  │ Cargos próprios  │  │ Cargos próprios│  │
│  └──────────────────┘  └──────────────────┘  └────────────────┘  │
│                                                                  │
│  Cada estabelecimento paga uma assinatura mensal (Plan)          │
└──────────────────────────────────────────────────────────────────┘
```

---

## Nível 1 — Plataforma (AdminSaas)

### AdminSaas — Administrador da Plataforma

É a conta administrativa da empresa que criou e opera o Mangefy.
**Não é o dono do restaurante.** Tem acesso total à plataforma e a todos os tenants.

**Responsabilidades:**
- Criar, ativar, suspender e cancelar restaurantes (Tenants)
- Gerenciar planos de assinatura (Basic, Pro, Enterprise...)
- Definir a matriz Plano × Tipo de Negócio (`PlanFeatureSet`) — quais features cada combinação tem acesso
- Criar e gerenciar tipos de negócio (Restaurante, Bar, Padaria...) e seus templates de cargo
- Criar e gerenciar o catálogo global de fornecedores (`PlatformSupplier`) e ramos de atuação (`SupplierCategory`)
- Visualizar métricas globais de uso da plataforma

> AdminSaas **não está no modelo de Employee**. É uma conta separada gerenciada pela própria plataforma.
> Entidades criadas pelo AdminSaas têm `CreatedByEmployeeId = null`.

---

## Nível 2 — Dentro do Estabelecimento (RBAC por Tenant)

Dentro de cada tenant existe um sistema de cargos **completamente independente**.
O que o Restaurante A configura **não interfere** no Restaurante B.

### Hierarquia de acesso

```
Tenant (estabelecimento)
│
├── TenantRole "Dono" (IsOwnerRole = true)
│     → todas as permissões implicitamente (HasPermission() sempre true)
│     → não pode ser editado, deletado ou desativado
│     → Employee com IsOwner=true não pode ser desativado ou ter cargo alterado
│
├── TenantRole "Gerente" (criado a partir de template ou pelo Dono)
│     Permissões: orders.read, orders.create, orders.cancel,
│                 tabs.read, tabs.create, tabs.close, tabs.cancel,
│                 menu.manage, tables.manage,
│                 employees.read, employees.manage,
│                 stock.read, stock.manage,
│                 cash.manage, reservations.manage,
│                 reports.read, settings.manage
│
├── TenantRole "Garçom" (template padrão)
│     Permissões: orders.read, orders.create,
│                 tabs.read, tabs.create,
│                 tables.read, menu.read,
│                 reservations.read
│
├── TenantRole "Cozinheiro" (template padrão)
│     Permissões: orders.read, orders.update_status, menu.read,
│                 stock.read
│
├── TenantRole "Caixa" (template padrão)
│     Permissões: tabs.read, tabs.close,
│                 orders.read, cash.manage,
│                 reports.read
│
└── TenantRole "Barman" (template padrão — tipo Bar)
      Permissões: orders.read, orders.create, orders.update_status,
                  tabs.read, tabs.create, menu.read, stock.read
```

> Os templates de cargo são definidos pelo AdminSaas por tipo de negócio.
> No onboarding, cada template vira um `TenantRole` independente (snapshot) — alterações
> futuras no template não afetam tenants já criados.

---

## Catálogo de Permissões

Definido pela plataforma em `PermissionCatalog`. O Owner atribui permissões a cada `TenantRole`.

### Comandas e Pedidos
| Permissão              | O que permite                                    |
|------------------------|--------------------------------------------------|
| `tabs.read`            | Ver comandas abertas e histórico                 |
| `tabs.create`          | Abrir nova comanda e atribuir número físico      |
| `tabs.close`           | Fechar comanda e registrar pagamento             |
| `tabs.cancel`          | Cancelar comanda                                 |
| `orders.read`          | Ver pedidos e tela KDS                           |
| `orders.create`        | Criar pedidos e adicionar itens                  |
| `orders.update_status` | Alterar status via KDS (Preparing → Ready)       |
| `orders.cancel`        | Cancelar pedidos e itens                         |

### Cardápio e Mesas
| Permissão              | O que permite                                    |
|------------------------|--------------------------------------------------|
| `menu.read`            | Ver cardápio e itens disponíveis                 |
| `menu.manage`          | Criar/editar/remover categorias, itens e fichas técnicas |
| `tables.read`          | Ver mesas e status                               |
| `tables.manage`        | Criar/editar/desativar mesas                     |

### Estoque e Fornecedores
| Permissão              | O que permite                                    |
|------------------------|--------------------------------------------------|
| `stock.read`           | Ver itens de estoque e movimentações             |
| `stock.manage`         | Registrar entradas, saídas e ajustes de inventário |

### Caixa e Reservas
| Permissão              | O que permite                                    |
|------------------------|--------------------------------------------------|
| `cash.manage`          | Abrir/fechar caixa e registrar sangrias          |
| `reservations.read`    | Ver reservas do dia                              |
| `reservations.manage`  | Criar/confirmar/cancelar reservas                |

### Funcionários e Relatórios
| Permissão              | O que permite                                    |
|------------------------|--------------------------------------------------|
| `employees.read`       | Ver funcionários e cargos                        |
| `employees.manage`     | Criar/editar/desativar funcionários              |
| `roles.read`           | Ver cargos e suas permissões                     |
| `roles.manage`         | Criar/editar/definir permissões de cargos        |
| `reports.read`         | Ver relatórios e analytics                       |
| `settings.manage`      | Configurar dados do estabelecimento              |

---

## Catálogo de Features (acesso por Plano × Tipo de Negócio)

Controlado pelo AdminSaas via `PlanFeatureSet`. O tenant acessa apenas o que seu plano+tipo permite.

| Feature Key                 | Módulo que libera                                         |
|-----------------------------|-----------------------------------------------------------|
| `features.tabs`             | Gestão de comandas e pedidos                              |
| `features.kds`              | Tela Kitchen Display System                               |
| `features.multi_menu`       | Múltiplos cardápios com vigência por horário              |
| `features.stock_basic`      | Controle de estoque: itens, movimentações, ficha técnica  |
| `features.stock_advanced`   | Relatórios de CMV, histórico de custo e desperdício       |
| `features.daily_cash`       | Abertura e fechamento de caixa com sangrias               |
| `features.reservations`     | Gestão de reservas de mesa                                |
| `features.reports_basic`    | Relatórios essenciais (vendas, comandas, caixa)           |
| `features.reports_advanced` | Analytics avançado (CMV, tendências, comparativos)        |
| `features.delivery`         | Módulo de delivery — integração futura (iFood, Rappi)     |
| `features.custom_roles`     | Criar cargos além dos templates padrão                    |

> Quando o AdminSaas remove uma feature da matriz, os tenants afetados têm **30 dias de carência**
> (`FeatureGracePeriod`) antes do bloqueio — e recebem notificação imediata.

---

## Regras do sistema de cargos

### 1. Owner não pode ser restringido
`TenantRole.IsOwnerRole = true` → `HasPermission()` sempre retorna `true`.
O cargo do Owner não pode ser editado. O Employee do Owner não pode ser desativado nem ter cargo alterado.

### 2. Escalada de privilégios bloqueada
Um Gerente com `roles.manage` só pode criar cargos com permissões que ele próprio possui.
Verificado na Application layer antes de chamar `TenantRole.SetPermissions()`.

### 3. Cargo com funcionários vinculados não pode ser deletado
A Application layer verifica se há Employees com o cargo antes de permitir exclusão.
O Owner deve reatribuir os funcionários primeiro.

### 4. Cargos customizados limitados por plano
`Plan.MaxCustomRoles` define o limite. Zero = plano não permite cargos customizados.
Em downgrade, cargos excedentes são desativados (`TenantRole.DeactivateByPlanDowngrade()`).
O Owner recebe lista dos funcionários afetados e deve reatribuir manualmente.

### 5. Turno e acesso temporário
O Owner configura o turno semanal de cada funcionário em `EmployeeSchedule`.
Ao término do turno, o funcionário entra em período de tolerância antes de ser bloqueado.
O Owner pode conceder acesso temporário via `Employee.GrantTemporaryAccess(until)`.

### 6. Isolamento por tenant
`TenantRole.TenantId` garante que toda query de cargos e permissões filtra pelo tenant atual.

---

## Fluxo de onboarding de um novo estabelecimento

```
1. AdminSaas cria o Tenant informando:
      → Name, Slug, Email, PlanId, BusinessTypeId, Timezone
      → Tenant.Status = TrialPeriod (14 dias grátis)
      → TenantCreatedEvent disparado

2. Application layer processa TenantCreatedEvent:
      a. Cria TenantRole "Dono" (IsOwnerRole=true)
      b. Para cada RoleTemplate ativo do BusinessType:
         → Cria TenantRole com snapshot das permissões (IsFromTemplate=true, TemplateId)
      c. Cria Employee do dono (IsOwner=true, Status=Active)
      d. Cria BusinessSchedule com grade semanal fechada por padrão
      e. Cria Menu padrão (IsDefault=true)
      f. Cria PaymentSettings, FiscalSettings, PrinterSettings,
         IntegrationSettings e TabSettings com valores padrão
      g. Cria Stock vazio

3. Dono recebe e-mail com link de primeiro acesso para definir senha.

4. Dono configura o estabelecimento:
      ├─ Define horário de funcionamento (BusinessSchedule)
      ├─ Configura ClosingPolicy (tolerância e ações bloqueadas)
      ├─ Edita permissões dos cargos padrão (opcional)
      ├─ Cria cargos customizados (se o plano permitir)
      ├─ Cadastra funcionários e define turnos (EmployeeSchedule)
      ├─ Configura o cardápio (Menu → Categorias → Itens + Ficha Técnica)
      ├─ Cadastra mesas
      ├─ Cadastra itens de estoque e vincula fornecedores
      ├─ Configura métodos de pagamento habilitados
      ├─ Define intervalo de numeração das comandas físicas (TabSettings)
      └─ Habilita NFC-e se tiver CNPJ e certificado digital (opcional)

5. Após trial → AdminSaas ativa o plano pago → Tenant.Status = Active
```
