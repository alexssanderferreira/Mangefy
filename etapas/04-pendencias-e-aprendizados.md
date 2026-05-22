# Pendências e Aprendizados — Mangefy

Registro de decisões ainda abertas, temas a pesquisar e ações necessárias
antes de implementar determinadas partes do sistema.

---

## DECISÕES ABERTAS

### [SETTINGS] Quais configurações são restritas por plano?
**Contexto:** Existem várias configurações e ainda não está totalmente definido quais são restritas por plano.
**Decidido:**
- NFC-e fiscal: disponível em todos os planos ✓
- Integrações de delivery: controladas por `features.delivery` ✓
- Impressoras: sem restrição de plano ✓
- XMLs fiscais para download: disponível a qualquer tenant com NFC-e ✓
**Ainda em aberto:** _(nenhum — itens abaixo foram decididos)_

**Reservas — limite de reservas simultâneas:**
Não é restrito por plano. O **Owner** define o limite diretamente nas configurações do estabelecimento. Será um novo aggregate `ReservationSettings` (padrão: sem limite). A Application layer verifica `ReservationSettings.MaxSimultaneousReservations` (nulo = ilimitado) antes de criar uma reserva.

**Caixa — histórico:**
**Ilimitado, sem restrição de plano.** A legislação brasileira (Receita Federal) exige guarda de documentos contábeis por 5 anos — limitar o histórico criaria risco legal para o tenant. O volume de dados é mínimo (~1 registro/dia). A diferença entre planos fica nos **relatórios analíticos** (`features.reports_advanced`), não nos dados brutos.

---

### [FISCAL] ⚠️ A IMPLEMENTAR — NFC-e (Nota Fiscal de Consumidor Eletrônica)
**Decisão tomada:** Opcional e configurável por tenant via `FiscalSettings.NfceEnabled`.
Não está vinculado a plano — qualquer tenant pode habilitar se tiver CNPJ e certificado digital.

**Como vai funcionar:**
```
Tab fechada → (se NfceEnabled = true) → chamar Hub Fiscal API
  → Hub envia para SEFAZ do estado
  → SEFAZ retorna chave de acesso (44 dígitos)
  → Sistema recebe XML autorizado
  → Impressora térmica imprime DANFE NFC-e (cupom com QR code)
```

**O que ainda precisa ser implementado:**
- [ ] Escolher e contratar hub fiscal (Focus NFe, NFe.io, Tecnospeed, Plugnotas)
- [ ] Estudar API do hub escolhido e mapear campos necessários
- [ ] Implementar `FiscalService` na camada Infrastructure
- [ ] Mapear dados da Tab/OrderItem para o formato exigido pelo hub
- [ ] Tratar falhas: SEFAZ offline, certificado vencido, rejeição por dados inválidos
- [ ] Implementar reemissão e cancelamento de NFC-e
- [ ] Armazenamento seguro do certificado A1 (.pfx) — cofre externo

**Escopo fora por ora (futuro):**
- NFS-e (Nota Fiscal de Serviço) — emitida pela prefeitura, muito fragmentada por município
- NF-e de produto para operações B2B

**Referências para quando for implementar:**
- Focus NFe: focusnfe.com.br/docs
- NFe.io: nfe.io/docs
- Tecnospeed: tecnospeed.com.br/nfce

---

### [SETTINGS] Certificado digital — como armazenar?
**Contexto:** O certificado A1 é um arquivo `.pfx` com senha. Armazená-lo no banco de
dados principal é um risco de segurança. O domínio deve guardar apenas uma referência.
**O que decidir:**
- Usar um serviço de cofre (Azure Key Vault, AWS Secrets Manager, HashiCorp Vault)?
- O tenant faz upload do certificado pelo sistema ou configura diretamente no hub fiscal?
- Como renovar o certificado antes do vencimento? (sistema alerta, owner renova)

---

### [SETTINGS] Credenciais de integrações — onde ficam?
**Contexto:** Tokens de iFood, Rappi, WhatsApp Business são dados sensíveis.
**O que decidir:**
- Mesmo cofre do certificado ou separado?
- O sistema Mangefy armazena os tokens ou redireciona para OAuth do próprio serviço?

---

### [FISCAL] Hub fiscal — qual contratar?
**Contexto:** Integração direta com SEFAZ de cada estado é inviável para um SaaS.
A solução padrão é usar um hub fiscal que abstrai a complexidade.
**Opções a pesquisar:**
- **Focus NFe** (focusnfe.com.br) — popular, bem documentado
- **NFe.io** — boa documentação REST
- **Tecnospeed** — robusto, usado por ERPs
- **Plugnotas** — simples de integrar
**O que decidir:**
- Qual hub contratar (avaliar preço por documento, SLA, suporte)
- Modelo de cobrança: Mangefy repassa o custo por NFC-e emitida ao tenant
  ou absorve no plano? (sugestão: cobrar por emissão, independente do plano)
- NFC-e é obrigatória para todos os tipos de negócio? (restaurantes sim, mas e padarias, bares?)

---

### [DELIVERY] ⚠️ A IMPLEMENTAR FUTURAMENTE — Integração com iFood/Rappi
**Decisão tomada:** Fora do escopo atual. O domínio já tem `features.delivery` no
`PlanFeatureSet` e `IntegrationSettings` com estrutura preparada. A lógica de
integração real será implementada em etapa futura.
**O que pesquisar quando chegar a hora:**
- iFood for Business / Merchant API (developer.ifood.com.br)
- Como pedido do iFood entra no sistema (Tab automática ou fila separada?)
- Sincronização de cardápio (itens do Mangefy → iFood)
- Como funciona o pagamento delivery (iFood já cobrou — registrar como pago automaticamente?)
- Rappi API equivalente

---

### [PAGAMENTOS] ⚠️ A IMPLEMENTAR FUTURAMENTE — Integração com maquininha e PIX dinâmico
**Decisão tomada:** Por ora o sistema apenas **registra como foi pago** — sem automação
com maquininha ou geração de QR code PIX. O funcionário seleciona o método de pagamento
habilitado pelo Owner e confirma manualmente. Integrações reais ficam para o futuro.
**O que pesquisar quando chegar a hora:**
- Stone Pagamentos API (integração com maquininha via TEF/SDK)
- PIX dinâmico via banco emissor (Banco Central — API do banco do tenant)
- Cielo, Rede como alternativas à Stone

---

### [HORÁRIO] ⚠️ A IMPLEMENTAR — Bloqueio por turno do funcionário
**Contexto:** Owner configura o turno semanal de cada funcionário via `EmployeeSchedule`.
**Decisões tomadas:**
- O turno nunca bloqueia no meio de uma operação. Ao término do turno, o funcionário entra
  em período de tolerância (mesmo conceito do `ClosingPolicy.GracePeriodMinutes`).
- O Owner pode liberar acesso temporário fora do turno para um funcionário específico.
**Decisões tomadas e implementadas:**

**Notificação de fim de turno:** Sim, com 10 minutos de antecedência. Usa o canal SignalR já planejado para o KDS. Um background job verifica turnos ativos e dispara notificação via hub. O frontend exibe banner persistente para o funcionário.

**Liberação de acesso temporário pelo Owner:**
O Owner acessa a tela **"Funcionários em turno"** (`GET api/tenants/{id}/employees/active`) que mostra quem está ativo e o horário previsto de fim de turno. Para cada funcionário, há um botão "Estender acesso" que chama `POST api/tenants/{id}/employees/{empId}/grant-access` com o campo `extensionMinutes`.

A Application layer calcula: `shiftEndTime + extensionMinutes` — o tempo é somado ao **horário de fim do turno**, não ao horário atual. O Owner pode estender várias vezes; cada extensão sobrescreve `Employee.TemporaryAccessUntil`.

**Período de tolerância do turno:** Configurável pelo Owner em `WorkforceSettings.ShiftToleranceMinutes` (padrão: 15 min), separado do `ClosingPolicy.GracePeriodMinutes` (que é do estabelecimento). Endpoint: `PUT api/tenants/{id}/settings/workforce`.

**O que ainda falta implementar:**
- [ ] Background job para notificação de fim de turno (SignalR)
- [ ] Tela "Funcionários em turno" (Frontend — Owner)
- [ ] Middleware de verificação de turno por request (API) — bloquear request se fora do turno e sem acesso temporário

---

### [IMPRESSORAS] ⚠️ A IMPLEMENTAR FUTURAMENTE — Fluxo de impressão
**Contexto:** Pode haver mais de uma impressora por estação.
**Decisões tomadas:**
- O sistema determina qual impressora usar por serviço: cozinha → impressora da cozinha, caixa → impressora do caixa.
  Cada estação tem uma impressora padrão (`Printer.IsDefault`). O funcionário não escolhe.
- A impressão via browser (QZ Tray ou agente local) é trabalho futuro — a implementar.
**O que decidir quando implementar:**
- Agente local (QZ Tray instalado no computador) ou serviço cloud (PrintNode)?
- Detecção de Online/Offline: ping periódico ou heartbeat do agente?
- Fallback: se a impressora padrão da estação estiver offline, usa a próxima da mesma estação ou alerta o funcionário?

---

### [IMPRESSORAS] ⚠️ A IMPLEMENTAR FUTURAMENTE — Download de XMLs fiscais
**Contexto:** Ao emitir NFC-e, o hub fiscal retorna um XML assinado pela SEFAZ.
Contadores exigem esses XMLs para escrituração fiscal mensal.
**Decisão:** Feature futura. Disponível para qualquer tenant com NFC-e habilitado (não restrita por plano).
**O que implementar quando chegar a hora:**
- [ ] Armazenar o XML retornado pelo hub fiscal junto ao registro da NFC-e
- [ ] Tela de download por período (filtro por mês/ano)
- [ ] Export em ZIP com todos os XMLs do período

---

### [INTEGRAÇÕES] ⚠️ A IMPLEMENTAR FUTURAMENTE — Google Meu Negócio
**Contexto:** Sincronizar horários de funcionamento do `BusinessSchedule` com o perfil
do estabelecimento no Google Maps automaticamente.
**Decisão:** Feature futura. Requer OAuth com a conta Google do Owner e integração com a
Google My Business API.
**O que pesquisar quando chegar a hora:**
- [ ] Google My Business API (developers.google.com/my-business)
- [ ] Fluxo OAuth para autenticação com conta Google do Owner
- [ ] Mapeamento entre `DaySchedule` e o formato de horários do Google
- [ ] Sincronização automática ao salvar `BusinessSchedule` (via Domain Event)

---

## DECISÕES TOMADAS (referência rápida)

| Tema | Decisão |
|------|---------|
| Tipos de negócio | Criados e gerenciados pelo AdminSaas |
| Templates de cargo | Por tipo de negócio, copiados no onboarding (snapshot) |
| Cargos customizados | Limitados por `Plan.MaxCustomRoles` |
| Downgrade de plano | Cargos customizados excedentes são desativados |
| Acesso a features | Matriz Plano × Tipo de Negócio (`PlanFeatureSet`) |
| Remoção de feature | 30 dias de carência (`FeatureGracePeriod`) com notificação |
| Comanda | Por pessoa, obrigatório nome do cliente |
| Mesa na comanda | Nullable — pode usar `LocationNote` livre |
| Mudança de mesa | `Tab.CurrentTableId` atualiza; pedidos anteriores mantêm mesa original |
| Pagamento parcial | Múltiplas formas de pagamento para uma comanda, soma deve igualar total |
| Transferência de itens | Não permitida entre comandas |
| Owner do tenant | Acesso irrestrito 24/7, cargo imutável |
| Fuso horário | Definido pelo AdminSaas na criação do tenant (IANA timezone) |
| Dias especiais | Fazem parte do `BusinessSchedule` |
| Métodos de pagamento | Apenas métodos habilitados em `PaymentSettings` aparecem no fechamento |
| Impressoras por estação | Múltiplas permitidas; sistema escolhe a padrão por estação (funcionário não escolhe) |
| Delivery (iFood/Rappi) | Habilitado via `features.delivery` no PlanFeatureSet — integração real é futura |
| Endereço do Tenant | Value object brasileiro (CEP, logradouro, número, complemento, bairro, cidade, UF) |
| Assinaturas | `Subscription` + `Invoice` — AdminSaas gera e confirma manualmente; automação é futura |
| NFC-e | Usar hub fiscal terceirizado (Focus NFe, NFe.io, etc.) — custo por emissão |
| XMLs fiscais | Download por período — feature futura, disponível a qualquer tenant com NFC-e |
| Google Meu Negócio | Sincronização de horários — feature futura (OAuth + Google My Business API) |
| Tela de Permissões | Não existe — gerenciamento de cargos/funcionários/acessos tem tela própria |
| Bloqueio por turno | Período de tolerância antes de bloquear; Owner estende acesso via `POST .../grant-access` com `extensionMinutes` somado ao fim do turno |
| Tolerância de turno | Configurável pelo Owner em `WorkforceSettings.ShiftToleranceMinutes` (padrão 15 min) via `PUT .../settings/workforce` |
| Notificação de fim de turno | 10 minutos antes via SignalR — background job monitora turnos ativos |
| Histórico de caixa | Ilimitado para todos os planos (obrigação legal de 5 anos); relatórios analíticos diferenciados por `features.reports_advanced` |
| Limite de reservas | Configurado pelo Owner em `ReservationSettings.MaxSimultaneousReservations` (nulo = sem limite); não restrito por plano |
| Baixa de estoque | Automática ao `OrderStatus.Ready`; cancelamentos anteriores = ajuste manual |
| Numeração de comandas | Números fixos configurados pelo Owner em `TabSettings`; reutilizáveis ao fechar |
| Múltiplos cardápios | Cardápio padrão sempre visível; adicionais com vigência por horário ou manual |
| Rastreabilidade | `CreatedByEmployeeId` / `UpdatedByEmployeeId` em toda `Entity`; nulo para AdminSaas |
| Fechamento de caixa | `CashRegister`: abertura, sangrias, fechamento com contagem física e cálculo de diferença |
| Reservas | Fluxo Pending→Confirmed→Arrived; chegada dispara abertura automática de comanda |

---

## APRENDIZADOS RECOMENDADOS

### NFC-e / Fiscal
- Estudar a documentação do **Focus NFe** ou **NFe.io** para entender o fluxo de integração
- Entender o que é DANFE NFC-e e quais campos são obrigatórios
- Pesquisar obrigatoriedade por estado e tipo de negócio (CNAE)
- Entender diferença entre NF-e (produto) e NFC-e (consumidor) e NFS-e (serviço)

### iFood for Business
- Pesquisar se o iFood disponibiliza API para integração com PDVs de terceiros
- Buscar documentação em: developer.ifood.com.br
- Entender modelo de parceria (iFood exige credenciamento de parceiros tecnológicos)

### Processamento de Pagamentos
- Estudar **Stone Pagamentos API** (stone.com.br/documentacao)
- Estudar **PIX dinâmico** via Banco Central (API do banco emissor)
- Avaliar se vale usar uma intermediadora como **Stripe** (internacional) ou manter nacionais

### Impressão Web
- Pesquisar soluções de impressão térmica via browser:
  - **QZ Tray** — agente local, imprime via JavaScript
  - **PrintNode** — serviço cloud de impressão
  - **Star Cloud Services** — para impressoras Star Micronics
