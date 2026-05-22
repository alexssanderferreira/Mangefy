# Decisões de Design — Mangefy

Histórico de dúvidas levantadas durante o design do sistema e as respostas definidas.
Este arquivo serve como referência para decisões futuras e para manter consistência
ao longo do desenvolvimento.

---

## Sistema de Cargos e Permissões

**Um funcionário pode ter mais de um cargo?**
> Não. Um funcionário tem apenas 1 cargo. Caso precise de uma combinação específica
> de permissões, o Owner cria um cargo exclusivo para aquele funcionário.

**O gerente pode criar outros funcionários?**
> Depende das permissões do seu cargo. Se o Owner atribuiu `employees.manage` ao cargo
> de Gerente, então sim. As permissões são configuradas pelo Owner e determinam o que
> cada cargo pode fazer.

**As permissões são granulares por tela ou por ação?**
> Por tela e por função dentro da tela. Exemplo: tela de Pedidos — o cargo pode ter
> permissão de leitura mas não de criar pedidos (`orders.read` vs `orders.create`).

**O Owner do restaurante pode ser restringido?**
> Não. O cargo do Owner (`IsOwnerRole = true`) tem todas as permissões implicitamente
> e não pode ser editado, deletado ou restringido.

**Deve haver cargos padrão ao criar um restaurante?**
> Sim. Ao criar um tenant, os cargos padrão do tipo de negócio são criados
> automaticamente a partir de templates gerenciados pelo AdminSaas.
> O Owner não precisa configurar cargos do zero.

**O Owner pode deletar os cargos padrão?**
> Não pode deletar. Pode simplesmente não usar — os cargos padrão ficam disponíveis
> na lista de seleção ao cadastrar funcionários, mas não obrigam uso.

**Criar cargos customizados é para todos os planos?**
> Não. Criar cargos além dos padrão é uma feature limitada por plano.
> O campo `Plan.MaxCustomRoles` define o limite. Zero significa que o plano
> não permite cargos customizados.

**O que acontece com cargos customizados em downgrade de plano?**
> São desativados (`TenantRole.IsActive = false`). O Owner recebe notificação
> com a lista de funcionários afetados e deve reatribuir manualmente cada um
> a um cargo ativo antes que o acesso seja bloqueado.

**AdminSaas pode deletar um template de cargo que está em uso?**
> Não. Se o template já originou `TenantRoles` com funcionários vinculados,
> o AdminSaas não pode deletá-lo. Pode apenas desativá-lo para novos tenants.

---

## Sistema de Comandas e Pedidos

**A comanda é por pessoa ou por grupo?**
> Sempre por pessoa. Cada comanda representa 1 cliente individualmente.
> Uma mesa pode ter N comandas abertas simultaneamente.

**Pode transferir itens entre comandas?**
> Não. Itens não podem ser movidos de uma comanda para outra.

**Como funciona o pagamento parcial?**
> Pagamento parcial existe apenas para dividir formas de pagamento,
> não para dividir entre pessoas. Exemplo: R$ 40 no Pix + R$ 40 no crédito.
> A soma dos pagamentos deve igualar o total da comanda ao fechar.

**Mesa é obrigatória para abrir uma comanda?**
> Não. A comanda pode ser aberta sem mesa — nesse caso deve ter uma
> `LocationNote` descritiva (ex: "Balcão do bar", "Área externa").
> Ao menos um dos dois (`TableId` ou `LocationNote`) é obrigatório.

**A comanda pode ser anônima?**
> Não. `CustomerName` é obrigatório em toda comanda.
> Serve para identificar o cliente em caso de perda da comanda
> ou de situações de inadimplência.

**Como funciona a mudança de mesa durante a permanência (cenário de bar)?**
> `Tab.CurrentTableId` é atualizado quando o cliente muda de mesa.
> Novos pedidos passam a referenciar a nova mesa.
> Pedidos anteriores já submetidos mantêm a mesa original para fins de entrega
> e histórico — não são alterados retroativamente.

---

## Tipos de Negócio e Funcionalidades por Plano

**Quem define os tipos de negócio?**
> Exclusivamente o AdminSaas. Tipos de negócio (Restaurante, Bar, Padaria...)
> são criados e gerenciados apenas na plataforma. O Owner do restaurante
> não tem acesso a essa configuração.

**O tipo de negócio afeta o sistema além dos templates de cargo?**
> Sim. A combinação Plano + Tipo de Negócio determina quais telas e
> funcionalidades o tenant tem acesso. Exemplo: Plano Basic + Padaria
> não dá acesso a KDS; Plano Pro + Restaurante dá acesso a estoque.

**Como modelar o acesso a funcionalidades: por plano, por tipo, ou combinação?**
> Combinação explícita — Opção B (Matriz Plano × Tipo de Negócio).
> O AdminSaas define, para cada combinação de Plano + Tipo de Negócio,
> exatamente quais features estão ativas (`PlanFeatureSet`).
> Isso dá controle total e preciso ao AdminSaas.

**O que acontece com tenants existentes quando o AdminSaas muda a matriz?**
> Opção 2 — Período de carência.
> A mudança na matriz gera um `FeatureGracePeriod` para cada tenant afetado.
> O tenant continua acessando a feature por 30 dias e recebe notificação.
> Após o prazo, o acesso é bloqueado automaticamente.
> Mudanças que adicionam features têm efeito imediato (sem carência necessária).

---

## Modelo de Negócio SaaS

**AdminSaas é o dono do restaurante?**
> Não. AdminSaas é o dono da plataforma Mangefy — a empresa que vende o sistema.
> O dono do restaurante é o `Employee` com `IsOwner = true`, vinculado ao seu tenant.
> AdminSaas não tem `TenantId` — sua conta é gerenciada fora do modelo de Employee.

**Qual é o fluxo de onboarding de um novo restaurante?**
> 1. AdminSaas cria o Tenant informando PlanId e BusinessTypeId
> 2. Sistema cria o TenantRole "Dono" (IsOwnerRole=true)
> 3. Sistema cria os TenantRoles a partir dos templates do BusinessType
> 4. Sistema cria o Employee do dono (IsOwner=true, Status=Active)
> 5. Owner recebe e-mail com link de primeiro acesso para definir senha
> 6. Owner configura cardápio, mesas e cadastra funcionários
> 7. Após trial de 14 dias → AdminSaas ativa o plano pago

---

## Pagamentos

**O tenant tem endereço cadastrado?**
> Sim. `Tenant.Address` é um value object brasileiro estruturado: CEP, logradouro, número,
> complemento (opcional), bairro, cidade e UF. O CEP é validado com 8 dígitos.
> Endereço é opcional no cadastro inicial e preenchido pelo Owner nas configurações.

**Como funciona a gestão de assinaturas dos tenants?**
> Módulo `Subscription` no lado da plataforma (AdminSaas). Cada tenant tem uma `Subscription`
> com lista de `Invoice` (faturas). O AdminSaas gera faturas (`GenerateInvoice`) e confirma
> pagamentos manualmente (`ConfirmPayment`). Um job agendado detecta faturas vencidas e
> chama `MarkOverdueInvoices()`, disparando `InvoiceOverdueEvent`.
> Integração com processadora de pagamento (Stripe, PagSeguro) é trabalho futuro.

**Como o sistema registra pagamentos?**
> Simples: o funcionário seleciona o método de pagamento habilitado pelo Owner
> e confirma manualmente o valor. O sistema apenas registra como foi pago —
> sem automação com maquininha, sem geração de QR code PIX, sem integração com Stone/Cielo.
> Automações e integrações com processadoras ficam para implementação futura.

**Como o Owner configura os métodos aceitos?**
> Via `PaymentSettings` nas configurações do tenant. O Owner habilita/desabilita
> cada método (PIX, Crédito, Débito, Dinheiro, Vale-refeição, etc.).
> Apenas métodos habilitados aparecem na tela de fechamento de comanda.
> O domínio `Payment` continua com enum fixo — a Application layer valida
> se o método está habilitado antes de registrar.

**Integração com delivery (iFood, Rappi) — quando?**
> Fora do escopo atual. O domínio já tem estrutura preparada (`features.delivery`,
> `IntegrationSettings`). A integração real será implementada em etapa futura
> após pesquisa das APIs dos parceiros.

---

## Fiscal

**NFC-e é obrigatória para todos os tenants?**
> Não. É opcional e configurável por tenant via `FiscalSettings.NfceEnabled`.
> Não está vinculada a nenhum plano de assinatura — qualquer tenant com CNPJ
> e certificado digital pode habilitar.
> Tenants MEI, informais ou enquadrados como serviço podem deixar desabilitado.

**Como o sistema emite NFC-e?**
> Via hub fiscal terceirizado (Focus NFe, NFe.io, Tecnospeed ou similar).
> O Mangefy envia os dados da venda para a API do hub; o hub cuida de toda
> a comunicação com a SEFAZ de cada estado e retorna o XML autorizado.
> O sistema então imprime o DANFE NFC-e na impressora térmica do caixa.
> Custo por emissão (~R$ 0,10–0,30) — não incluído no plano de assinatura.

**NFS-e (Nota Fiscal de Serviço) será suportada?**
> Fora do escopo inicial. NFS-e é emitida pela prefeitura de cada município,
> o que torna a integração extremamente fragmentada. Pode ser avaliada como
> evolução futura mediante demanda dos clientes.

---

## Decisões Técnicas de Domínio

**Validações ficam na entidade ou em serviços externos?**
> Na própria entidade via `DomainException`. A entidade nunca entra em estado
> inválido independente de quem chamar o método.

**Como criar entidades sem expor o construtor?**
> Construtores são `private`. Entidades são criadas via factory methods
> (`Create()`, `Open()`, `CreateOwner()`). EF Core usa o construtor privado
> para reconstituição — suprimimos o warning CS8618 no `.csproj`.

**Menu é um agregado separado ou parte do Tenant?**
> Menu é um Aggregate Root separado com `TenantId`. Um tenant pode ter múltiplos Menus.
> O cardápio padrão (`IsDefault=true`) é sempre visível. Cardápios adicionais têm vigência
> automática por `MenuSchedule` ou ativação manual. Categorias e itens são entidades internas
> ao agregado Menu — só acessíveis através do Menu.

**Como funciona a numeração de comandas físicas?**
> O Owner configura um intervalo em `TabSettings` (ex: 1–80). Os números correspondem a fichas
> físicas reutilizáveis — ao fechar a comanda o número volta ao pool imediatamente.
> A Application layer consulta comandas abertas para determinar números disponíveis.

**Como é feita a rastreabilidade de autoria dos registros?**
> `Entity` base tem `CreatedByEmployeeId` e `UpdatedByEmployeeId` (`Guid?`).
> Entidades da plataforma (AdminSaas) ficam com `null` — apenas entidades do tenant rastreiam autor.

**Table deve guardar referência às comandas abertas?**
> Não. Table não conhece as comandas. Ela é notificada via Domain Events:
> `TabOpenedEvent` → `Table.Occupy()` (idempotente — suporta N comandas).
> `TabClosedEvent` → Application layer verifica se restam comandas abertas
> → se não → `Table.Release()`.

**Os templates de cargo são copiados ou referenciados no tenant?**
> Copiados (snapshot). Ao criar o tenant, cada template vira um `TenantRole`
> independente com `IsFromTemplate=true` e `TemplateId` como rastreabilidade.
> Alterações futuras no template não afetam tenants já criados.
> Isso garante que o Owner tenha controle total sobre seus próprios cargos.

**O snapshot de preço e nome no OrderItem é necessário?**
> Sim. `ItemName` e `UnitPrice` são copiados do MenuItem no momento do pedido.
> Se o cardápio for alterado depois, o histórico de pedidos permanece correto.

---

## Estoque

**O estoque controla ingredientes ou produtos prontos?**
> Ingredientes. Produtos vendidos como estão (ex: vinho, cerveja) são tratados como
> ingredientes de si mesmos — sem distinção especial no modelo.

**Como o estoque é baixado?**
> Automaticamente ao vender um item do cardápio: a Application layer lê a ficha técnica
> (`RecipeIngredient`) do `MenuItem` e chama `Stock.DeductForSale()` para cada ingrediente.
> Movimentações manuais também são possíveis: entrada (compra), saída (consumo, perda/vencimento)
> e ajuste de inventário.

**O estoque é global ou por setor (cozinha, bar)?**
> Global por tenant. O `StockItem` possui `Station` (Kitchen, Bar, Custom) para filtros,
> mas o estoque em si é único — não há estoques separados por setor.

**Fornecedor é uma entidade ou campo livre?**
> Entidade. Dois níveis: `PlatformSupplier` (catálogo global gerenciado pelo AdminSaas,
> somente leitura para o tenant) e `Supplier` (lista do tenant — pode vir do catálogo
> ou ser cadastrado manualmente pelo Owner). Apenas fornecedores manuais são editáveis pelo Owner.

**Ramo de atuação do fornecedor é texto livre ou lista controlada?**
> `SupplierCategory` com dois níveis: globais (TenantId = null, criadas pelo AdminSaas)
> e exclusivas do tenant (TenantId preenchido, criadas pelo Owner).

**Pedido ao fornecedor entra no sistema?**
> Não no momento. Feature futura registrada em pendências.

---

## Caixa, Reservas e Múltiplos Cardápios

**Quando o estoque é baixado automaticamente?**
> Ao marcar o pedido como Pronto (`OrderStatus.Ready`), via `OrderReadyEvent`.
> Se um item for cancelado antes disso, a cozinha faz ajuste manual no estoque.
> Não há reversão automática de baixa por cancelamento.

**Como funciona o fechamento de caixa?**
> O operador abre o caixa com um valor inicial (troco). Durante o turno registra sangrias.
> No fechamento, conta o dinheiro físico. O sistema calcula:
> `Esperado = abertura + vendas em dinheiro − sangrias`.
> A diferença (sobra/falta) é registrada no `CashRegisterClosedEvent`.

**Como funciona o fluxo de reservas?**
> Reservation: Pending → Confirmed → Arrived (vincula TabId) | Cancelled | NoShow.
> A API recebe `POST /reservations/{id}/arrival` com `{ employeeId }`.
> `RegisterArrivalCommandHandler` valida o status, abre a Tab via `OpenTabCommand` (com fallback `LocationNote = "Reserva"` se não houver mesa), e depois chama `Reservation.RegisterArrival(tabId)` internamente.
> `ReservationArrivedEvent` é informativo — reservado para notificações futuras.

**Múltiplos cardápios ativos ao mesmo tempo são possíveis?**
> Sim. O cardápio padrão sempre está visível. Cardápios adicionais podem ter `MenuSchedule`
> (vigência por dias/horário) ou ser ativados manualmente. Todos os ativos são exibidos juntos.
> Não há conflito — o cliente vê todos os itens disponíveis no momento.
