# Mangefy — Status do Projeto

> Baseado na leitura direta do código-fonte em 2026-07-05.

---

## O que é o sistema

**Mangefy** é um SaaS multi-tenant para gestão de restaurantes e bares.

Existem dois perfis de usuário:

- **AdminSaas** — a empresa Mangefy. Gerencia owners, tenants, planos, assinaturas e toda a configuração da plataforma.
- **Tenant (Owner + Funcionários)** — cada restaurante/bar. Operam comandas, cardápio, estoque, caixa e reservas.

### Fluxo de onboarding
1. AdminSaas cria um **Owner** (pessoa física/jurídica).
2. Owner recebe e-mail para ativar a conta e definir senha.
3. AdminSaas cria um **Tenant** (restaurante) vinculado ao Owner.
4. Tenant recebe as roles padrão do tipo de negócio (ex: garçom, gerente, cozinheiro).
5. AdminSaas cria a **Assinatura** com o plano escolhido.
6. Owner faz login → seleciona o restaurante → acessa o painel.

---

## Estado atual (por área)

### Backend — **Substancialmente completo**

O backend cobre todos os agregados do domínio com Domain, Application e API. Os gaps são pontuais.

| Área | Estado |
|---|---|
| Auth (login, switch-tenant, ativação) | ✅ Completo |
| AdminSaas (owners, tenants, planos, assinaturas, fornecedores, feature matrix) | ✅ Completo |
| Tenant — Mesas, Cardápio, Comandas, Pedidos | ✅ Completo |
| Tenant — Estoque, Caixa, Reservas | ✅ Completo |
| Tenant — Funcionários, Cargos, Sessões Operacionais | ✅ Completo |
| Tenant — Configurações (pagamento, fiscal, impressoras, horários, etc.) | ✅ Completo |
| Tenant — Supplier (fornecedor do tenant) | ❌ Sem handlers e sem controller |
| Tenant — IntegrationSettings (iFood, Rappi) | ❌ Sem handlers e sem controller |

### Frontend Admin `/admin/**` — **Completo**

Todas as telas de gestão da plataforma existem e funcionam: owners, tenants, planos, assinaturas, tipos de negócio, feature matrix, fornecedores.

### Frontend Tenant `/app/**` — **Não existe**

Só há um dashboard placeholder com "em breve". O backend está completo mas não tem interface.

---

## O que falta construir

### Prioridade 1 — Núcleo operacional do tenant (sem isso o sistema não serve ao restaurante)

1. **Shell do tenant** — sidebar, topbar, rotas base em `/app`
2. **Mesas** (`/app/tables`) — visualizar status, criar, editar
3. **Cardápio** (`/app/menus`) — menus, categorias, itens, receitas
4. **Comandas** (`/app/tabs`) — abrir, adicionar pedidos, fechar com pagamento, cancelar

### Prioridade 2 — Operações de suporte

5. **Funcionários** (`/app/employees`) — listar, criar, editar, desativar
6. **Cargos** (`/app/roles`) — criar cargos com permissões personalizadas
7. **KDS** (`/app/kds`) — tela da cozinha para acompanhar pedidos em preparo
8. **Funcionários em turno** — quem está ativo, estender acesso temporário
9. **Tela de criação de Tenant no Admin** — o painel admin não tem botão para criar novo restaurante

### Prioridade 3 — Funcionalidades adicionais

10. **Caixa** (`/app/cash-register`) — abertura, sangrias, fechamento com conferência
11. **Reservas** (`/app/reservations`) — calendário, confirmação, registro de chegada
12. **Estoque** (`/app/stock`) — inventário, compras, ajustes
13. **Relatórios** (`/app/reports`) — vendas e operacional com filtro de data
14. **Configurações do tenant** (`/app/settings`) — horários, formas de pagamento, impressoras

### Backend pendente

15. **Supplier do tenant** — criar, listar, editar fornecedores do restaurante (domínio existe, falta Application + API + repositório)
16. **IntegrationSettings** — handlers e controller para configurar integrações de delivery (iFood, Rappi)

---

## Problemas encontrados no código (já corrigidos nessa sessão)

| # | Problema | Status |
|---|---|---|
| 1 | `OperationalSessionsController.Start` e `End` sem `[RequirePermission]` — qualquer usuário autenticado podia iniciar sessões | ✅ Corrigido |
| 2 | `SubmitOrderItemRequest` tinha campos `ItemName`, `UnitPrice`, `RequiresKds` no DTO público que eram silenciosamente ignorados pelo handler — risco de injeção de preço por dev futuro | ✅ Corrigido |
| 3 | `FeatureGracePeriod.Create()` exigia `gracePeriodDays > 0` mas `PlanFeatureSet.RemoveFeature()` permitia `0` — bug de runtime ao remover feature sem carência | ✅ Corrigido |
| 4 | Tag `</thinking>` inválida em docstring gerada por IA | ✅ Corrigido |
| 5 | `UpdateMenuItemCommandValidator` exigia `Price > 0`, mas `AddMenuItemCommandValidator` aceitava `>= 0` — item gratuito criado não podia ser editado de volta para gratuito | ✅ Corrigido |
| 6 | `MenuCategory.AddItem()` ignorava o parâmetro `Station` — todos os itens criados iam para a estação `Kitchen` independente da categoria | ✅ Corrigido |

---

## Problemas conhecidos ainda abertos

### Segurança

| # | Problema | Impacto |
|---|---|---|
| 1 | Sem `HasQueryFilter` no EF Core — isolação de tenant depende de cada query incluir `TenantId` manualmente | Médio (todos os handlers já checam pós-fetch, mas sem defense-in-depth no DB) |
| 2 | JWT do AdminSaas usa `sub = "adminsaas"` (string literal) em vez de um Guid real | Baixo (problema de auditoria/revogação) |
| 3 | Token armazenado em `localStorage` — vulnerável a XSS | Baixo (padrão de SPA, decisão consciente) |
| 4 | Sem rate limiting no login e resolve-tenants | Médio (força-bruta não bloqueada) |

### Backend

| # | Problema | Impacto |
|---|---|---|
| 5 | `ISupplierRepository` existe no domínio mas sem implementação e sem registro no DI — crash em runtime se usado | Alto |
| 6 | Dois `SubscriptionsController` com lógicas diferentes para o mesmo aggregate | Médio (confusão de manutenção) |
| 7 | `ReservationArrivedEventHandler` é stub vazio — chegada de reserva não cria comanda automaticamente | Médio |
| 8 | 17 domain events sem handler — `InvoiceOverdueEvent` (sem suspensão automática), `OrderSubmittedEvent` (sem print job para KDS), `StockLowEvent` (sem alerta), `TenantSuspendedEvent` (sem bloqueio de acesso), entre outros | Médio a Alto |
| 9 | `UnitOfWork` não preenche `CreatedByEmployeeId` quando a ação é de Owner ou AdminSaas | Baixo |
| 10 | `Money.Multiply(int)` — não suporta quantidades fracionárias em receitas (ex: 0.5 porção) | Baixo |
| 11 | `DeliveryInfo.PhoneNumber` é `string?` em vez do Value Object `PhoneNumber` — sem validação de telefone em pedidos delivery | Baixo |

### Frontend Admin

| # | Problema | Impacto |
|---|---|---|
| 12 | 7 componentes injetam `HttpClient` diretamente em vez de usar services — `TenantDetailComponent`, `ActivateOwnerComponent`, `AdminTopbarComponent`, etc. | Baixo |
| 13 | Chamada ao ViaCEP duplicada em 3 componentes — falta um `ViaCepService` centralizado | Baixo |
| 14 | `authGuard` não preserva a URL atual ao redirecionar para login — usuário perde contexto após relogar | Baixo |
| 15 | Sem refresh automático de token (TTL de 8h) — após expirar, usuário é deslogado sem aviso | Baixo |

---

## Funcionalidades planejadas para o futuro (fora do escopo atual)

- **NFC-e** — emissão de nota fiscal via hub (Focus NFe / NFe.io). Requer escolha e contratação do hub, certificado A1, integração com SEFAZ.
- **iFood / Rappi** — integração de delivery. A estrutura no domínio (`IntegrationSettings`, `features.delivery`) já existe.
- **Pagamento automatizado** — maquininha Stone, PIX dinâmico via banco. Hoje o sistema só registra o método de pagamento informado manualmente.
- **Impressão térmica** — QZ Tray ou PrintNode para impressão de comandas e KDS.
- **Notificação de fim de turno** — background job + SignalR para avisar o funcionário 10 minutos antes do turno encerrar.
- **Bloqueio por turno** — middleware para bloquear requests de funcionários fora do horário de trabalho.
- **Google Meu Negócio** — sincronizar `BusinessSchedule` com o perfil no Google Maps.
