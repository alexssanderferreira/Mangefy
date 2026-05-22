# Briefing de Design Frontend — Mangefy
**Para usar no Claude.ai — Design iterativo de telas**

---

## O que é o Mangefy

Mangefy é um **sistema SaaS multi-tenant de gestão de restaurantes**. Uma empresa (AdminSaas) opera a plataforma e vende acesso a múltiplos estabelecimentos (Tenants). Cada estabelecimento é um mundo isolado com seus próprios funcionários, cardápio, mesas, comandas e configurações.

**Stack frontend:** Angular + TypeScript  
**API:** REST JSON (ASP.NET Core 8)  
**Realtime:** SignalR (KDS e status de mesas)  
**Autenticação:** JWT Bearer

---

## Os dois mundos da aplicação

### Mundo 1 — Painel Admin SaaS
Usado exclusivamente pela empresa que opera o Mangefy.  
**URL base:** `/admin/...`  
**Acesso:** conta separada, login em `/auth/admin/login`

### Mundo 2 — Painel do Estabelecimento
Usado pelos donos e funcionários de cada restaurante.  
**URL base:** `/app/...`  
**Acesso:** login em `/auth/login` com e-mail + senha

---

## MUNDO 1 — Painel Admin SaaS

### Quem usa
O administrador da plataforma Mangefy. Não é o dono do restaurante.  
Tem acesso total a todos os tenants.

### Módulos e telas

#### 1. Dashboard
- Cards com totais: tenants ativos, em trial, suspensos, cancelados
- Lista de faturas em atraso (nome do tenant, valor, dias em atraso)
- Lista de trials prestes a vencer (dias restantes)
- Tabela de tenants recentes

#### 2. Tenants
**Lista de tenants**
- Colunas: nome, slug, tipo de negócio, plano, status, data de criação
- Filtros: status (Active / TrialPeriod / Suspended / Cancelled), plano, tipo de negócio
- Botão "Novo Tenant"
- Clique na linha abre o detalhe

**Detalhe do tenant**
- Informações: nome, slug, e-mail, timezone, endereço
- Plano atual + data de vencimento do trial
- Tipo de negócio
- Status com botões de ação:
  - Se Active/Trial → botão "Suspender"
  - Se Suspended → botão "Reativar"
  - Se não Cancelled → botão "Cancelar" (destrutivo, pedir confirmação)
- Aba "Assinatura": histórico de faturas do tenant
- Aba "Funcionários": lista somente leitura dos funcionários do tenant

**Criar Tenant (modal ou página)**
- Campos: Nome*, Slug* (auto-gerado a partir do nome, editável), E-mail do dono*, Plano*, Tipo de Negócio*, Timezone (padrão: America/Sao_Paulo)
- Slug: apenas letras minúsculas, números e hifens — validação em tempo real
- Ao criar: sistema gera automaticamente cargos, cardápio padrão, configurações

#### 3. Planos
**Lista de planos**
- Colunas: nome, preço mensal, limites (mesas, itens, usuários, cargos custom), status
- Botão "Novo Plano"

**Criar / Editar Plano**
- Campos: Nome*, Preço mensal*, Max mesas, Max itens de cardápio, Max usuários, Max cargos customizados (0 = não permite)
- Status: Ativo / Inativo

#### 4. Assinaturas
**Lista de faturas**
- Filtros: status (Pending / Paid / Overdue / Cancelled), tenant, período
- Colunas: tenant, valor, vencimento, status, referência de pagamento
- Botão "Gerar Fatura" (seleciona tenant + valor + vencimento)
- Botão "Confirmar Pagamento" em faturas Pending/Overdue (campo opcional: referência)

#### 5. Tipos de Negócio
**Lista de tipos** (Restaurante, Bar, Padaria, etc.)
- Botão "Novo Tipo"
- Toggle ativo/inativo por tipo (inativo não pode ser usado em novos tenants)

**Detalhe do tipo**
- Nome do tipo
- Lista de templates de cargo:
  - Nome do template, permissões atribuídas, ativo/inativo
  - Botão "Adicionar template"
  - Editar permissões de cada template (checklist das permissões disponíveis)
  - Atenção: template com tenants vinculados só pode ser desativado, não deletado

#### 6. Matriz de Features (Plano × Tipo de Negócio)
- Tabela cruzada: linhas = planos, colunas = tipos de negócio
- Célula mostra quais features estão ativas para aquela combinação
- Clique na célula abre painel de edição com checklist das features:
  - `features.tabs`, `features.kds`, `features.multi_menu`
  - `features.stock_basic`, `features.stock_advanced`
  - `features.daily_cash`, `features.reservations`
  - `features.reports_basic`, `features.reports_advanced`
  - `features.delivery`, `features.custom_roles`
- Aviso ao remover feature: "Tenants afetados terão 30 dias de carência"

#### 7. Fornecedores da Plataforma
- Lista de fornecedores globais (catálogo que os tenants podem consultar)
- Colunas: nome, categoria, ativo/inativo
- CRUD completo

#### 8. Categorias de Fornecedor (globais)
- Lista de categorias globais (Bebidas, Carnes, Hortifruti, etc.)
- CRUD simples

---

## MUNDO 2 — Painel do Estabelecimento

### Perfis de utilizador e o que cada um vê

| Perfil | Acesso típico |
|--------|---------------|
| **Owner (Dono)** | Tudo. Sem restrição de permissão. |
| **Manager (Gerente)** | Depende das permissões do cargo configurado pelo Owner |
| **Waiter (Garçom)** | Mesas, comandas, pedidos |
| **Cashier (Caixa)** | Comandas, fechamento, caixa |
| **Kitchen (Cozinheiro)** | KDS — tela de produção |

> O sistema é baseado em RBAC por cargo. O Owner configura as permissões de cada cargo.  
> A UI deve esconder/desabilitar itens de menu e ações que o utilizador não tem permissão.

### Módulos e telas

#### 1. Login do estabelecimento
- E-mail + senha
- Redireciona para o módulo principal do cargo (ex: Cozinheiro vai direto para KDS)

#### 2. Dashboard / Home
- Resumo do dia: comandas abertas, total vendido, caixa aberto/fechado
- Alertas: estoque baixo, reservas do dia, trials a vencer
- Atalhos para os módulos principais

#### 3. Mesas
**Permissão: `tables.read`**
- Grade visual de mesas por setor
- Status visual por cor:
  - Verde = Disponível
  - Vermelho = Ocupada
  - Azul = Reservada
  - Cinza = Indisponível
- Clique na mesa: ver comandas abertas nela
- Com `tables.manage`: botão para criar/editar mesa

**Criar / Editar Mesa**
- Número*, Capacidade*, Setor (opcional: "Salão", "Área externa", etc.)
- Status: Available / Unavailable

#### 4. Comandas (Tabs)
**Permissão: `tabs.read`**
- Lista de comandas abertas
- Colunas: número, cliente, mesa/localização, total atual, tempo aberta
- Filtros: por mesa, por status

**Abrir Comanda**
**Permissão: `tabs.create`**
- Nome do cliente*, Número da comanda* (dentro do intervalo configurado), Mesa (dropdown de mesas disponíveis) ou Nota de localização
- Ao menos mesa OU nota de localização obrigatório

**Detalhe da Comanda**
- Cabeçalho: cliente, número, mesa, horário de abertura
- Lista de pedidos agrupados por ordem de envio
- Itens com nome, quantidade, preço unitário, status
- Total parcial em tempo real
- Botões de ação:
  - "Novo Pedido" (abre seletor de itens do cardápio)
  - "Mudar Mesa" (com `tabs.create`)
  - "Fechar Comanda" (com `tabs.close`)
  - "Cancelar Comanda" (com `tabs.cancel`, pede motivo)

**Novo Pedido (dentro da comanda)**
- Navegação por categorias do cardápio
- Busca por nome de item
- Adicionar itens com quantidade
- Observação por item (opcional)
- Botão "Enviar para cozinha/bar"

**Fechar Comanda**
**Permissão: `tabs.close`**
- Total da comanda
- Selecionar forma(s) de pagamento (apenas métodos habilitados em PaymentSettings)
- Divisão de valores se múltiplos métodos (soma deve igualar o total)
- Botão confirmar

#### 5. KDS — Kitchen Display System
**Permissão: `orders.read`** (feature: `features.kds`)
- Tela dividida por estação (Cozinha / Bar / Custom)
- Cards de pedidos com: número da comanda, cliente, mesa, itens, tempo desde envio
- Status dos itens com botões de ação:
  - "Iniciar" (Submitted → InProgress)
  - "Pronto" (InProgress → Ready)
- Pedidos prontos ficam destacados até o garçom marcar como entregue
- Atualização em tempo real via **SignalR**

#### 6. Cardápio (Menu)
**Permissão: `menu.read` / `menu.manage`**

**Lista de cardápios**
- Cardápio padrão sempre visível no topo
- Cardápios adicionais com badge de status (Ativo / Inativo / Com vigência)
- Com `menu.manage`: botão "Novo Cardápio" (requer feature `features.multi_menu`)

**Detalhe do cardápio**
- Lista de categorias com itens
- Expandir/recolher categorias
- Status de cada item (Disponível / Indisponível / Sem estoque) — badge colorido
- Com `menu.manage`: botões para adicionar categoria, adicionar item, editar, remover

**Criar / Editar Item do Cardápio**
- Nome*, Preço*, Estação (Kitchen / Bar / Custom)*, Descrição (opcional), Foto (opcional)
- Status: Available / Unavailable / OutOfStock
- Aba "Ficha Técnica": lista de ingredientes do estoque com quantidade (feature `features.stock_basic`)

#### 7. Estoque
**Permissão: `stock.read` / `stock.manage`** (feature: `features.stock_basic`)

**Lista de itens do estoque**
- Colunas: nome, unidade, quantidade atual, mínimo, custo, setor, fornecedor
- Alerta visual quando quantidade <= mínimo (destaque em vermelho)
- Filtros: setor, fornecedor, alertas de estoque baixo
- Com `stock.manage`: botões de ação por item (entrada, saída, ajuste)

**Criar Item de Estoque**
- Nome*, Unidade (kg, L, un, etc.)*, Quantidade atual*, Estoque mínimo*, Custo por unidade*, Setor*, Fornecedor (opcional)

**Registrar Entrada (Compra)**
- Item, Quantidade*, Fornecedor (opcional), Observação (opcional)

**Registrar Saída Manual**
- Item, Quantidade*, Tipo (Consumo / Perda)*, Motivo* (obrigatório)

**Ajuste de Inventário**
- Item, Nova quantidade*, Motivo* (obrigatório)
- Exibe diferença calculada (ex: "−3 unidades")

**Histórico de movimentações**
- Tabela com: data, item, tipo, quantidade, motivo, autor

#### 8. Fornecedores
**Permissão: `stock.manage`**

**Lista de fornecedores do tenant**
- Distingue visualmente: catálogo da plataforma vs cadastro manual
- Fornecedores do catálogo: somente nome do representante e notas são editáveis
- Fornecedores manuais: totalmente editáveis

**Adicionar fornecedor**
- Opção 1: buscar no catálogo da plataforma e adicionar
- Opção 2: cadastrar manualmente (nome, categoria, contato)

#### 9. Caixa (Daily Cash)
**Permissão: `cash.manage`** (feature: `features.daily_cash`)

**Estado do caixa (tela principal)**
- Se fechado: botão "Abrir Caixa" (pede valor inicial)
- Se aberto:
  - Valor de abertura, total de vendas em dinheiro, total de sangrias
  - Saldo esperado calculado
  - Botão "Registrar Sangria"
  - Botão "Fechar Caixa"

**Fechar Caixa**
- Campo: "Valor contado fisicamente"
- Exibe: esperado vs contado, diferença (sobra em verde / falta em vermelho)
- Botão confirmar

**Histórico de caixas**
- Lista de registros anteriores com: data, abertura, fechamento, diferença

#### 10. Reservas
**Permissão: `reservations.read` / `reservations.manage`** (feature: `features.reservations`)

**Lista de reservas do dia**
- Visualização em linha do tempo ou lista por horário
- Status visual: Pendente (amarelo), Confirmada (azul), Chegou (verde), Cancelada (cinza), NoShow (vermelho)
- Com `reservations.manage`: botão "Nova Reserva"

**Criar Reserva**
- Nome do cliente*, Nº de pessoas*, Data e hora*, Mesa (opcional), Telefone (opcional), Observações

**Ações por reserva**
- Confirmar (Pending → Confirmed)
- Registrar Chegada (→ Arrived) — abre comanda automaticamente com dados da reserva
- Cancelar (pede motivo)
- Marcar como NoShow

#### 11. Funcionários
**Permissão: `employees.read` / `employees.manage`**

**Lista de funcionários**
- Colunas: nome, e-mail, cargo, status, último acesso
- Filtros: cargo, status

**Criar Funcionário**
- Nome*, E-mail*, Cargo* (dropdown dos cargos ativos do tenant)
- Ao criar: funcionário recebe e-mail de ativação para definir senha

**Detalhe do funcionário**
- Dados gerais
- Cargo atual (editável pelo Owner)
- Status: Active / PendingActivation / Inactive
- Botão "Desativar" (Owner não pode desativar a si mesmo)
- Aba "Turno": grade semanal com horários por dia
- Aba "Acesso Temporário": conceder acesso fora do turno com data de expiração

#### 12. Cargos (Roles)
**Permissão: `roles.read` / `roles.manage`**

**Lista de cargos**
- Nome, tipo (padrão do template / customizado), nº de funcionários vinculados, status
- Badge especial para o cargo Owner (não editável)
- Com `roles.manage`: botão "Novo Cargo" (limitado por `Plan.MaxCustomRoles`)

**Editar permissões do cargo**
- Checklist agrupado por módulo:
  - Comandas: tabs.read, tabs.create, tabs.close, tabs.cancel
  - Pedidos: orders.read, orders.create, orders.update_status, orders.cancel
  - Cardápio: menu.read, menu.manage
  - Mesas: tables.read, tables.manage
  - Estoque: stock.read, stock.manage
  - Caixa: cash.manage
  - Reservas: reservations.read, reservations.manage
  - Funcionários: employees.read, employees.manage
  - Cargos: roles.read, roles.manage
  - Relatórios: reports.read
  - Configurações: settings.manage
- Cargo Owner: checklist desabilitado (todas as permissões implícitas)

#### 13. Relatórios
**Permissão: `reports.read`** (feature: `features.reports_basic`)

**Relatório de Vendas**
- Filtro de período (hoje, semana, mês, customizado)
- Total de vendas, nº de comandas, ticket médio
- Breakdown por método de pagamento
- Itens mais vendidos

**Relatório de Caixa**
- Histórico de aberturas e fechamentos
- Diferenças (sobras e faltas)

**Relatório Avançado** (feature: `features.reports_advanced`)
- CMV (Custo de Mercadoria Vendida)
- Análise de desperdício de estoque
- Tendências e comparativos por período

#### 14. Configurações
**Permissão: `settings.manage`**

Organizado em abas:

**Estabelecimento**
- Nome, e-mail de contato, telefone, endereço (CEP, logradouro, número, bairro, cidade, UF)
- Timezone

**Horário de Funcionamento**
- Grade semanal: cada dia da semana com toggle Aberto/Fechado e campos de horário
- Dias Especiais: calendário de feriados e horários excepcionais (com motivo obrigatório)
- Política de Fechamento: tolerância em minutos, ações bloqueadas no período de tolerância

**Métodos de Pagamento**
- Lista de métodos com toggle: Pix, Crédito, Débito, Dinheiro, Vale-refeição, etc.
- Pelo menos um deve estar habilitado

**Comandas**
- Intervalo de numeração física: mínimo e máximo (padrão 1–50)

**Impressoras**
- Lista de impressoras por estação (Cozinha, Bar, Caixa, Custom)
- Cadastro: nome, IP, porta, estação
- Marcar como padrão por estação

**Fiscal (NFC-e)**
- Toggle habilitar NFC-e (requer CNPJ + chave API do hub fiscal)
- CNPJ, Chave da API, Ambiente (Homologação / Produção)
- Toggle "Emitir automaticamente ao fechar comanda"

**Integrações** (placeholder para futuro)
- Delivery (iFood, Rappi) — em breve

---

## Regras de UI importantes

### Controlo de acesso
- Menu lateral exibe apenas módulos que o utilizador tem permissão de ver
- Botões de ação (criar, editar, deletar) são ocultados se o utilizador não tem permissão
- Nunca mostrar mensagem "403 Forbidden" — simplesmente não mostrar o elemento

### Feature gates
- Se o tenant não tem a feature, o módulo inteiro não aparece no menu
- Ex: sem `features.kds` → sem item "KDS" no menu

### Multi-tenant
- O JWT carrega `tenantId` e `employeeId`
- Toda chamada de API inclui o tenant implicitamente (via JWT, não via URL)
- Nunca expor dados de outros tenants

### Feedback visual
- Loading states em toda chamada assíncrona
- Toast de sucesso / erro após ações
- Confirmação modal para ações destrutivas (cancelar comanda, suspender tenant, etc.)
- Campos com erro de validação com mensagem inline

### Realtime (SignalR)
- KDS: pedidos novos aparecem automaticamente
- Mapa de mesas: status atualiza em tempo real
- Dashboard: contadores de comandas abertas atualizam sem refresh

---

## Fluxos críticos para priorizar no design

### Fluxo operacional principal (Garçom/Caixa)
```
Login → Ver mesas → Abrir comanda → Selecionar itens → Enviar pedido
→ Acompanhar status (KDS) → Fechar comanda → Registrar pagamento
```

### Fluxo de onboarding (Dono, após criar conta)
```
Definir senha (link e-mail) → Login → Configurar horários → Criar cardápio
→ Criar mesas → Cadastrar funcionários → Pronto para operar
```

### Fluxo AdminSaas
```
Login admin → Criar tenant → (sistema gera cargos e configs automaticamente)
→ Tenant recebe e-mail → Trial 14 dias → AdminSaas ativa plano pago
```

---

## Paleta e estilo sugeridos

- **Tom geral:** profissional, denso de informação, não infantil
- **KDS:** alto contraste, fonte grande, legível à distância (tela de cozinha)
- **Mobile-friendly:** garçom usa tablet ou smartphone
- **Desktop-first:** admin, manager e relatórios são usados em desktop

---

## Endpoints da API disponíveis (resumo)

Todos os endpoints estão implementados e funcionais:

| Módulo | Base URL |
|--------|----------|
| Auth | `POST /api/auth/login`, `POST /api/auth/admin/login`, `POST /api/auth/set-password` |
| Tenants | `GET/POST /api/tenants`, `GET/PUT /api/tenants/{id}`, `POST /api/tenants/{id}/suspend` |
| Planos | `GET/POST /api/admin/plans`, `PUT /api/admin/plans/{id}` |
| Assinaturas | `GET/POST /api/subscriptions`, `POST /api/subscriptions/{id}/confirm-payment` |
| Tipos de negócio | `GET/POST /api/admin/business-types` |
| Feature sets | `GET/PUT /api/admin/plan-feature-sets` |
| Funcionários | `GET/POST /api/employees`, `PUT /api/employees/{id}`, `POST /api/employees/{id}/deactivate` |
| Cargos | `GET/POST /api/roles`, `PUT /api/roles/{id}/permissions` |
| Cardápio | `GET/POST /api/menus`, `POST /api/menus/{id}/categories`, `POST /api/menus/{id}/items` |
| Mesas | `GET/POST /api/tables`, `PUT /api/tables/{id}` |
| Comandas | `GET/POST /api/tabs`, `GET /api/tabs/{id}`, `POST /api/tabs/{id}/close`, `POST /api/tabs/{id}/cancel` |
| Pedidos | `POST /api/tabs/{id}/orders`, `POST /api/tabs/{tabId}/orders/{orderId}/submit` |
| KDS | `POST /api/tabs/{tabId}/orders/{orderId}/items/{itemId}/start`, `.../ready` |
| Estoque | `GET/POST /api/stock`, `POST /api/stock/{id}/purchase`, `POST /api/stock/{id}/adjust` |
| Caixa | `GET/POST /api/cash-registers`, `POST /api/cash-registers/{id}/close`, `POST /api/cash-registers/{id}/withdrawal` |
| Reservas | `GET/POST /api/reservations`, `POST /api/reservations/{id}/confirm`, `POST /api/reservations/{id}/arrival` |
| Relatórios | `GET /api/reports/sales`, `GET /api/reports/operational` |
| Configurações | `GET/PUT /api/settings/payment`, `GET/PUT /api/settings/fiscal`, `GET/PUT /api/settings/printers`, `GET/PUT /api/settings/tabs` |

---

*Briefing gerado em 17/05/2026 — Mangefy v1.0*
