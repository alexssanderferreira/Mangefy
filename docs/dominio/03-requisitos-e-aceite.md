# Requisitos e Critérios de Aceite — Mangefy

Documento de referência para desenvolvimento e QA. Organizado por módulo.
Cada requisito tem um código único (`RF-XXX`) e seus critérios de aceite (`CA-XXX`).

---

## Convenções

- **RF** — Requisito Funcional
- **CA** — Critério de Aceite
- **[Owner]** — ação executada pelo dono do estabelecimento
- **[Employee]** — ação executada por funcionário com permissão adequada
- **[AdminSaas]** — ação executada pelo administrador da plataforma

---

## 1. Plataforma — Tenants

### 1.1 Criação de Tenant
**RF-001** [AdminSaas] Criar um novo estabelecimento na plataforma.

| # | Critério de Aceite |
|---|-------------------|
| CA-001 | Nome, slug, e-mail, plano e tipo de negócio são obrigatórios |
| CA-002 | Slug deve conter apenas letras minúsculas, números e hifens |
| CA-003 | Slug deve ser único na plataforma |
| CA-004 | Tenant é criado com status `TrialPeriod` e trial de 14 dias |
| CA-005 | Fuso horário padrão é `America/Sao_Paulo` se não informado |
| CA-006 | Ao criar, o sistema gera automaticamente: cargo Dono, cargos dos templates do tipo de negócio, Employee do dono, Menu padrão, BusinessSchedule, configurações padrão (Payment, Fiscal, Printer, Tab, Integration) e estoque vazio |
| CA-007 | O dono recebe e-mail com link de primeiro acesso |

### 1.2 Gestão do Tenant
**RF-002** [AdminSaas] Ativar, suspender e cancelar tenants.

| # | Critério de Aceite |
|---|-------------------|
| CA-008 | Tenant suspenso não pode ter funcionários autenticados |
| CA-009 | Tenant cancelado não pode ser reativado |
| CA-010 | Suspensão dispara `TenantSuspendedEvent` |
| CA-011 | Cancelamento dispara `TenantCancelledEvent` |

### 1.3 Endereço do Tenant
**RF-003** [Owner] Cadastrar endereço do estabelecimento.

| # | Critério de Aceite |
|---|-------------------|
| CA-012 | CEP deve ter exatamente 8 dígitos numéricos |
| CA-013 | Logradouro, número, bairro, cidade e UF são obrigatórios |
| CA-014 | UF deve ter exatamente 2 caracteres, normalizado para maiúsculas |
| CA-015 | Complemento é opcional |

---

## 2. Planos e Features

### 2.1 Gestão de Planos
**RF-004** [AdminSaas] Criar e gerenciar planos de assinatura.

| # | Critério de Aceite |
|---|-------------------|
| CA-016 | Plano deve ter nome, preço mensal e limites (mesas, itens, usuários, cargos customizados) |
| CA-017 | `MaxCustomRoles = 0` significa que o plano não permite cargos customizados |

### 2.2 Matriz de Features (Plano × Tipo de Negócio)
**RF-005** [AdminSaas] Definir quais features cada combinação Plano+Tipo tem acesso.

| # | Critério de Aceite |
|---|-------------------|
| CA-018 | Feature só pode ser adicionada se existir no `FeatureCatalog` |
| CA-019 | Ao remover uma feature, tenants afetados recebem 30 dias de carência (`FeatureGracePeriod`) |
| CA-020 | Adição de feature tem efeito imediato — sem carência |
| CA-021 | Durante a carência, o tenant continua acessando a feature normalmente |
| CA-022 | Após expirar a carência, o acesso é bloqueado automaticamente |

### 2.3 Troca de Plano
**RF-006** [AdminSaas] Alterar o plano de um tenant.

| # | Critério de Aceite |
|---|-------------------|
| CA-023 | Em downgrade, cargos customizados excedentes são desativados |
| CA-024 | Owner recebe lista de funcionários afetados pelo downgrade |
| CA-025 | `TenantPlanChangedEvent` é disparado na troca |

---

## 3. Assinaturas e Faturas

### 3.1 Geração de Faturas
**RF-007** [AdminSaas] Gerar faturas mensais de assinatura para os tenants.

| # | Critério de Aceite |
|---|-------------------|
| CA-026 | Valor da fatura deve ser maior que zero |
| CA-027 | Fatura é criada com status `Pending` |
| CA-028 | Cada tenant tem exatamente uma `Subscription` |

### 3.2 Confirmação de Pagamento
**RF-008** [AdminSaas] Confirmar pagamento de uma fatura manualmente.

| # | Critério de Aceite |
|---|-------------------|
| CA-029 | Fatura já paga não pode ser paga novamente |
| CA-030 | Ao confirmar, `NextDueDate` da assinatura é atualizado |
| CA-031 | Referência do pagamento (boleto, transferência) é opcional mas registrável |
| CA-032 | `InvoicePaidEvent` é disparado ao confirmar |

### 3.3 Faturas em Atraso
**RF-009** [Sistema] Detectar e marcar automaticamente faturas vencidas.

| # | Critério de Aceite |
|---|-------------------|
| CA-033 | Apenas faturas `Pending` com data de vencimento anterior a hoje são marcadas como `Overdue` |
| CA-034 | `InvoiceOverdueEvent` é disparado para cada fatura marcada |
| CA-035 | Painel do AdminSaas exibe tenants com faturas em atraso |

---

## 4. Tipos de Negócio e Templates de Cargo

### 4.1 Tipos de Negócio
**RF-010** [AdminSaas] Criar e gerenciar tipos de negócio.

| # | Critério de Aceite |
|---|-------------------|
| CA-036 | Tipo de negócio tem nome e pode ter múltiplos templates de cargo |
| CA-037 | Tipo desativado não pode ser usado em novos tenants |

### 4.2 Templates de Cargo
**RF-011** [AdminSaas] Definir templates de cargo por tipo de negócio.

| # | Critério de Aceite |
|---|-------------------|
| CA-038 | Permissões do template devem existir no `PermissionCatalog` |
| CA-039 | Template com tenants vinculados não pode ser deletado — apenas desativado |
| CA-040 | No onboarding, cada template ativo gera um `TenantRole` independente (snapshot) |
| CA-041 | Alterações futuras no template não afetam tenants já criados |

---

## 5. Cargos e Permissões (RBAC)

### 5.1 Criação de Cargos
**RF-012** [Owner] Criar cargos customizados para o estabelecimento.

| # | Critério de Aceite |
|---|-------------------|
| CA-042 | Criação de cargo customizado requer `features.custom_roles` no plano |
| CA-043 | Número de cargos customizados não pode exceder `Plan.MaxCustomRoles` |
| CA-044 | Nome do cargo é obrigatório |

### 5.2 Edição de Permissões
**RF-013** [Owner] Definir permissões de cada cargo.

| # | Critério de Aceite |
|---|-------------------|
| CA-045 | Permissões devem existir no `PermissionCatalog` |
| CA-046 | Cargo do Owner (`IsOwnerRole = true`) não pode ser editado |
| CA-047 | Funcionário com `roles.manage` só pode atribuir permissões que ele próprio possui |
| CA-048 | Cargo inativo não pode ser editado — deve ser reativado primeiro |

### 5.3 Exclusão de Cargos
**RF-014** [Owner] Remover cargos não utilizados.

| # | Critério de Aceite |
|---|-------------------|
| CA-049 | Cargo com funcionários vinculados não pode ser deletado |
| CA-050 | Cargo do Owner não pode ser deletado |
| CA-051 | Owner deve reatribuir os funcionários antes de deletar o cargo |

---

## 6. Funcionários

### 6.1 Cadastro de Funcionários
**RF-015** [Owner/Gerente com `employees.manage`] Cadastrar funcionários.

| # | Critério de Aceite |
|---|-------------------|
| CA-052 | Nome, e-mail e cargo são obrigatórios |
| CA-053 | E-mail deve ser único dentro do tenant |
| CA-054 | Funcionário é criado com status `PendingActivation` |
| CA-055 | Funcionário recebe e-mail com link de ativação e definição de senha |
| CA-056 | Cada funcionário tem exatamente 1 cargo |

### 6.2 Ativação e Desativação
**RF-016** [Owner] Ativar ou desativar funcionários.

| # | Critério de Aceite |
|---|-------------------|
| CA-057 | O Employee do Owner não pode ser desativado |
| CA-058 | Funcionário desativado não consegue autenticar |
| CA-059 | Funcionário inativo não pode ser ativado diretamente — deve passar por `PendingActivation` se necessário |

### 6.3 Atribuição de Cargo
**RF-017** [Owner] Alterar o cargo de um funcionário.

| # | Critério de Aceite |
|---|-------------------|
| CA-060 | Cargo do Owner não pode ser alterado |
| CA-061 | Novo cargo deve existir e estar ativo |

### 6.4 Acesso Temporário
**RF-018** [Owner] Conceder acesso temporário fora do turno.

| # | Critério de Aceite |
|---|-------------------|
| CA-062 | Data de término deve ser no futuro |
| CA-063 | Acesso temporário expira automaticamente ao atingir a data |
| CA-064 | Owner pode revogar o acesso temporário manualmente |

---

## 7. Horário de Funcionamento

### 7.1 Grade Semanal
**RF-019** [Owner] Configurar horário de funcionamento por dia da semana.

| # | Critério de Aceite |
|---|-------------------|
| CA-065 | Cada dia pode ser configurado como aberto (com horário) ou fechado |
| CA-066 | Horário de abertura deve ser anterior ao de fechamento |
| CA-067 | Por padrão, todos os dias são criados como fechados |

### 7.2 Dias Especiais
**RF-020** [Owner] Cadastrar feriados e dias com horário diferente.

| # | Critério de Aceite |
|---|-------------------|
| CA-068 | Dia especial sobrepõe a grade semanal na data correspondente |
| CA-069 | Feriado marca o estabelecimento como fechado naquele dia |
| CA-070 | Dia especial com horário customizado deve ter abertura anterior ao fechamento |
| CA-071 | Motivo é obrigatório para feriados e dias especiais |
| CA-072 | Apenas um dia especial por data é permitido — cadastrar novo substitui o anterior |

### 7.3 Política de Fechamento
**RF-021** [Owner] Configurar o que acontece quando o horário de funcionamento termina.

| # | Critério de Aceite |
|---|-------------------|
| CA-073 | Período de tolerância não pode ser negativo |
| CA-074 | Owner define quais ações são bloqueadas durante o período de tolerância |
| CA-075 | Padrão: 30 min de tolerância, bloqueando `tabs.create` e `orders.create` |

---

## 8. Turno dos Funcionários

### 8.1 Configuração de Turno
**RF-022** [Owner] Definir o turno semanal de cada funcionário.

| # | Critério de Aceite |
|---|-------------------|
| CA-076 | Cada dia pode ser dia de trabalho (com horário) ou folga |
| CA-077 | Horário de início deve ser anterior ao de fim |
| CA-078 | Por padrão, todos os dias são folga |

### 8.2 Bloqueio por Turno
**RF-023** [Sistema] Bloquear acesso fora do turno.

| # | Critério de Aceite |
|---|-------------------|
| CA-079 | Ao término do turno, funcionário entra em período de tolerância antes de ser bloqueado |
| CA-080 | Funcionário com acesso temporário ativo (`TemporaryAccessUntil`) pode operar fora do turno |
| CA-081 | Owner tem acesso irrestrito independente de turno |

---

## 9. Cardápio

### 9.1 Criação e Gestão de Cardápios
**RF-024** [Owner/`menu.manage`] Gerenciar cardápios do estabelecimento.

| # | Critério de Aceite |
|---|-------------------|
| CA-082 | Todo tenant tem exatamente um cardápio padrão (`IsDefault = true`) criado no onboarding |
| CA-083 | Cardápio padrão não pode ser desativado |
| CA-084 | Cardápio padrão não pode ter horário de vigência (`MenuSchedule`) |
| CA-085 | Criar cardápios adicionais requer `features.multi_menu` |
| CA-086 | Cardápios adicionais podem ter vigência automática por dias da semana e faixa de horário |
| CA-087 | Múltiplos cardápios podem estar ativos simultaneamente — todos são exibidos juntos |

### 9.2 Categorias e Itens
**RF-025** [Owner/`menu.manage`] Gerenciar categorias e itens do cardápio.

| # | Critério de Aceite |
|---|-------------------|
| CA-088 | Nome da categoria é obrigatório |
| CA-089 | Categoria com itens não pode ser removida |
| CA-090 | Item deve ter nome, preço e setor (`Kitchen`, `Bar`, `Custom`) |
| CA-091 | Preço não pode ser negativo |
| CA-092 | Item pode ser marcado como `Available`, `Unavailable` ou `OutOfStock` |

### 9.3 Ficha Técnica (Receita)
**RF-026** [Owner/`menu.manage`] Vincular ingredientes do estoque a itens do cardápio.

| # | Critério de Aceite |
|---|-------------------|
| CA-093 | Cada ingrediente da ficha técnica referencia um `StockItem` por Id |
| CA-094 | Quantidade do ingrediente deve ser maior que zero |
| CA-095 | Item sem ficha técnica não gera baixa automática no estoque |

---

## 10. Mesas

### 10.1 Gestão de Mesas
**RF-027** [Owner/`tables.manage`] Cadastrar e gerenciar mesas.

| # | Critério de Aceite |
|---|-------------------|
| CA-096 | Mesa deve ter número e capacidade |
| CA-097 | Mesa pode ter setor (ex: "Área externa", "Salão") |
| CA-098 | Status da mesa: `Available`, `Occupied`, `Reserved`, `Unavailable` |

### 10.2 Ocupação de Mesa
**RF-028** [Sistema] Atualizar status da mesa via eventos.

| # | Critério de Aceite |
|---|-------------------|
| CA-099 | Mesa passa para `Occupied` ao receber `TabOpenedEvent` (idempotente — suporta N comandas) |
| CA-100 | Mesa volta para `Available` apenas quando não restar nenhuma comanda aberta |
| CA-101 | Mesa passa para `Reserved` ao ter uma reserva confirmada para o mesmo dia |

---

## 11. Comandas (Tabs)

### 11.1 Abertura de Comanda
**RF-029** [Employee/`tabs.create`] Abrir uma nova comanda.

| # | Critério de Aceite |
|---|-------------------|
| CA-102 | Nome do cliente é obrigatório |
| CA-103 | Comanda deve ter mesa (`TableId`) ou descrição de localização (`LocationNote`) — pelo menos um |
| CA-104 | Número da comanda deve estar dentro do intervalo configurado em `TabSettings` |
| CA-105 | Número deve estar disponível (nenhuma comanda aberta com o mesmo número) |
| CA-106 | Ao abrir, dispara `TabOpenedEvent` → mesa passa para `Occupied` |

### 11.2 Mudança de Mesa
**RF-030** [Employee/`tabs.create`] Mover cliente para outra mesa durante a permanência.

| # | Critério de Aceite |
|---|-------------------|
| CA-107 | `Tab.CurrentTableId` é atualizado com a nova mesa |
| CA-108 | Pedidos já submetidos mantêm a mesa original — não são alterados retroativamente |
| CA-109 | Novos pedidos após a mudança referenciam a nova mesa |

### 11.3 Fechamento de Comanda
**RF-031** [Employee/`tabs.close`] Fechar comanda e registrar pagamento.

| # | Critério de Aceite |
|---|-------------------|
| CA-110 | Soma dos pagamentos deve igualar o total da comanda ao fechar |
| CA-111 | Método de pagamento deve estar habilitado em `PaymentSettings` |
| CA-112 | Múltiplas formas de pagamento são permitidas (ex: R$40 Pix + R$40 crédito) |
| CA-113 | Ao fechar, número da comanda volta ao pool de disponíveis imediatamente |
| CA-114 | Se NFC-e habilitada e `AutoEmitOnTabClose = true`, emite nota automaticamente |
| CA-115 | `TabClosedEvent` é disparado → Application layer verifica se há outras comandas abertas na mesa → se não → mesa liberada |

### 11.4 Cancelamento de Comanda
**RF-032** [Employee/`tabs.cancel`] Cancelar comanda.

| # | Critério de Aceite |
|---|-------------------|
| CA-116 | Motivo do cancelamento é obrigatório |
| CA-117 | Comanda fechada não pode ser cancelada |
| CA-118 | `TabCancelledEvent` é disparado |

---

## 12. Pedidos (Orders / KDS)

### 12.1 Criação de Pedido
**RF-033** [Employee/`orders.create`] Adicionar itens a um pedido dentro de uma comanda.

| # | Critério de Aceite |
|---|-------------------|
| CA-119 | Pedido só pode ser criado em comanda aberta |
| CA-120 | Item deve estar `Available` no cardápio |
| CA-121 | Snapshot de nome e preço do item é copiado no momento do pedido |
| CA-122 | Quantidade deve ser maior que zero |

### 12.2 Envio para Cozinha/Bar
**RF-034** [Employee/`orders.create`] Submeter pedido para produção.

| # | Critério de Aceite |
|---|-------------------|
| CA-123 | Pedido deve ter pelo menos 1 item para ser submetido |
| CA-124 | Ao submeter, `OrderSubmittedEvent` é disparado |
| CA-125 | Itens com `RequiresKds = true` aparecem no KDS da estação correspondente |

### 12.3 Atualização de Status (KDS)
**RF-035** [Employee/`orders.update_status`] Atualizar status dos pedidos na tela KDS.

| # | Critério de Aceite |
|---|-------------------|
| CA-126 | Fluxo de status: `Submitted → InProgress → Ready → Delivered` |
| CA-127 | Ao marcar como `Ready`, `OrderReadyEvent` é disparado |
| CA-128 | Application layer processa `OrderReadyEvent`: para cada item com ficha técnica, deduz estoque |
| CA-129 | Se estoque cair abaixo do mínimo após a dedução, `StockLowEvent` é disparado |

### 12.4 Cancelamento de Item
**RF-036** [Employee/`orders.cancel`] Cancelar itens de um pedido.

| # | Critério de Aceite |
|---|-------------------|
| CA-130 | Item só pode ser cancelado antes de o pedido ser marcado como `Ready` |
| CA-131 | Cancelamento após `Ready` requer ajuste manual no estoque |
| CA-132 | Item cancelado não é cobrado na comanda |

---

## 13. Estoque

### 13.1 Cadastro de Itens
**RF-037** [Owner/`stock.manage`] Cadastrar itens no estoque.

| # | Critério de Aceite |
|---|-------------------|
| CA-133 | Item deve ter nome, unidade, quantidade atual, estoque mínimo, custo e setor |
| CA-134 | Quantidade atual e estoque mínimo não podem ser negativos |
| CA-135 | Custo por unidade deve ser maior que zero |
| CA-136 | Item pode ser vinculado a um fornecedor |

### 13.2 Movimentações de Entrada
**RF-038** [Employee/`stock.manage`] Registrar entrada de mercadoria (compra).

| # | Critério de Aceite |
|---|-------------------|
| CA-137 | Quantidade deve ser maior que zero |
| CA-138 | Motivo é opcional para entradas |
| CA-139 | Movimentação é registrada com tipo `Purchase` |

### 13.3 Movimentações de Saída Manual
**RF-039** [Employee/`stock.manage`] Registrar saída manual (consumo, perda, vencimento).

| # | Critério de Aceite |
|---|-------------------|
| CA-140 | Motivo é obrigatório para saídas manuais |
| CA-141 | Quantidade não pode exceder o estoque disponível |
| CA-142 | Tipos disponíveis: `ManualConsumption`, `Loss` |

### 13.4 Ajuste de Inventário
**RF-040** [Owner/`stock.manage`] Ajustar quantidade após contagem física.

| # | Critério de Aceite |
|---|-------------------|
| CA-143 | Motivo é obrigatório |
| CA-144 | Nova quantidade não pode ser negativa |
| CA-145 | Sistema registra a diferença (positiva ou negativa) como `InventoryAdjustment` |

### 13.5 Alerta de Estoque Mínimo
**RF-041** [Sistema] Notificar quando item atingir estoque mínimo.

| # | Critério de Aceite |
|---|-------------------|
| CA-146 | `StockLowEvent` é disparado quando `CurrentQuantity <= MinimumQuantity` |
| CA-147 | Alerta ocorre em deduções automáticas (venda) e manuais (saída, ajuste) |

---

## 14. Fornecedores

### 14.1 Catálogo da Plataforma
**RF-042** [AdminSaas] Gerenciar catálogo global de fornecedores.

| # | Critério de Aceite |
|---|-------------------|
| CA-148 | Fornecedor deve ter nome e categoria (ramo de atuação) |
| CA-149 | Tenant pode consultar o catálogo mas não pode editar dados do catálogo |
| CA-150 | Tenant pode adicionar fornecedor do catálogo à sua lista com `AddFromPlatform()` |

### 14.2 Fornecedores do Tenant
**RF-043** [Owner/`stock.manage`] Gerenciar lista de fornecedores do estabelecimento.

| # | Critério de Aceite |
|---|-------------------|
| CA-151 | Fornecedor manual é totalmente editável pelo Owner |
| CA-152 | Fornecedor do catálogo: apenas nome do representante e notas podem ser editados |
| CA-153 | Categoria pode ser global (AdminSaas) ou exclusiva do tenant (criada pelo Owner) |

---

## 15. Configurações

### 15.1 Métodos de Pagamento
**RF-044** [Owner/`settings.manage`] Habilitar e desabilitar métodos de pagamento.

| # | Critério de Aceite |
|---|-------------------|
| CA-154 | Pelo menos um método deve permanecer habilitado |
| CA-155 | Apenas métodos habilitados aparecem na tela de fechamento de comanda |

### 15.2 Configurações Fiscais
**RF-045** [Owner/`settings.manage`] Habilitar emissão de NFC-e.

| # | Critério de Aceite |
|---|-------------------|
| CA-156 | CNPJ e chave de API do hub fiscal são obrigatórios para habilitar NFC-e |
| CA-157 | `AutoEmitOnTabClose` só pode ser ativado se NFC-e estiver habilitada |
| CA-158 | Desabilitar NFC-e desativa também `AutoEmitOnTabClose` |

### 15.3 Impressoras
**RF-046** [Owner/`settings.manage`] Cadastrar impressoras por estação.

| # | Critério de Aceite |
|---|-------------------|
| CA-159 | Impressora deve ter nome, endereço IP/porta e estação (`Kitchen`, `Bar`, `Cashier`, `Custom`) |
| CA-160 | Primeira impressora cadastrada por estação torna-se padrão automaticamente |
| CA-161 | Impressora padrão de uma estação pode ser alterada pelo Owner |
| CA-162 | Ao remover a impressora padrão, a próxima da mesma estação assume o papel |

### 15.4 Numeração de Comandas
**RF-047** [Owner/`settings.manage`] Configurar intervalo de numeração das comandas físicas.

| # | Critério de Aceite |
|---|-------------------|
| CA-163 | Número mínimo deve ser maior que zero |
| CA-164 | Número máximo deve ser maior que o mínimo |
| CA-165 | Intervalo padrão: 1 a 50 |

---

## 16. Caixa (Daily Cash)

### 16.1 Abertura de Caixa
**RF-048** [Employee/`cash.manage`] Abrir o caixa no início do turno.

| # | Critério de Aceite |
|---|-------------------|
| CA-166 | Valor de abertura (troco inicial) não pode ser negativo |
| CA-167 | Não pode haver dois caixas abertos simultaneamente para o mesmo tenant |
| CA-168 | `CashRegisterOpenedEvent` é disparado |

### 16.2 Sangria
**RF-049** [Employee/`cash.manage`] Registrar retirada de dinheiro do caixa.

| # | Critério de Aceite |
|---|-------------------|
| CA-169 | Valor da sangria deve ser maior que zero |
| CA-170 | Motivo da sangria é obrigatório |
| CA-171 | Sangria só pode ser registrada com caixa aberto |

### 16.3 Fechamento de Caixa
**RF-050** [Employee/`cash.manage`] Fechar o caixa ao fim do turno.

| # | Critério de Aceite |
|---|-------------------|
| CA-172 | Operador informa o valor contado fisicamente |
| CA-173 | Sistema calcula: `Esperado = abertura + vendas em dinheiro − sangrias` |
| CA-174 | Diferença (sobra/falta) é exibida e registrada |
| CA-175 | `CashRegisterClosedEvent` é disparado com valores esperado, contado e diferença |

---

## 17. Reservas

### 17.1 Criação de Reserva
**RF-051** [Employee/`reservations.manage`] Cadastrar reserva de mesa.

| # | Critério de Aceite |
|---|-------------------|
| CA-176 | Nome do cliente é obrigatório |
| CA-177 | Número de pessoas deve ser maior que zero |
| CA-178 | Data não pode ser no passado |
| CA-179 | Mesa é opcional — reserva pode ser sem mesa pré-definida |
| CA-180 | Reserva criada com status `Pending` |

### 17.2 Fluxo de Status da Reserva
**RF-052** [Employee/`reservations.manage`] Gerenciar status da reserva.

| # | Critério de Aceite |
|---|-------------------|
| CA-181 | `Pending → Confirmed`: confirmação da presença do cliente |
| CA-182 | `Pending/Confirmed → Arrived`: cliente chegou — vincula `TabId` e abre comanda |
| CA-183 | Ao registrar chegada, `ReservationArrivedEvent` é disparado → comanda aberta automaticamente |
| CA-184 | `Pending/Confirmed → Cancelled`: motivo obrigatório |
| CA-185 | `Pending/Confirmed → NoShow`: cliente não compareceu |
| CA-186 | Reserva `Arrived`, `Cancelled` ou `NoShow` não pode ser editada |

---

## 18. Relatórios

### 18.1 Relatórios Básicos
**RF-053** [Employee/`reports.read`] Visualizar relatórios essenciais.

| # | Critério de Aceite |
|---|-------------------|
| CA-187 | Disponível para tenants com `features.reports_basic` |
| CA-188 | Exibe: total de vendas por período, comandas abertas/fechadas, métodos de pagamento utilizados |
| CA-189 | Relatório de caixa: histórico de aberturas e fechamentos com diferenças |

### 18.2 Relatórios Avançados
**RF-054** [Employee/`reports.read`] Visualizar analytics avançado.

| # | Critério de Aceite |
|---|-------------------|
| CA-190 | Disponível para tenants com `features.reports_advanced` |
| CA-191 | Exibe: CMV (Custo de Mercadoria Vendida), itens mais vendidos, desperdício de estoque |

---

## 19. Regras Transversais

| # | Critério de Aceite |
|---|-------------------|
| CA-192 | Todo funcionário autenticado deve pertencer a um tenant ativo (`Active` ou `TrialPeriod`) |
| CA-193 | Toda operação de escrita deve verificar permissão do cargo antes de executar |
| CA-194 | Features controladas por plano devem ser verificadas antes de liberar acesso ao módulo |
| CA-195 | Todas as entidades do tenant registram `CreatedByEmployeeId` e `UpdatedByEmployeeId` |
| CA-196 | Isolamento multi-tenant: nenhuma query retorna dados de outro tenant |
| CA-197 | Owner tem acesso a todas as funções do tenant independente de permissão |
| CA-198 | Horário de funcionamento deve ser verificado via timezone do tenant, nunca UTC direto |
