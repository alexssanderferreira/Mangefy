# Implementacao Offline Futura - Modo Continuidade

Este documento registra a proposta futura para manter o Mangefy operando quando a
internet da loja cair. A ideia nao e tornar todo o SaaS offline, mas garantir a
continuidade da operacao essencial do restaurante: vender, preparar, cobrar,
imprimir e sincronizar depois.

Status: futuro, fora do escopo atual de backend.

---

## Objetivo

Criar um **Modo Continuidade Offline** para que a loja continue funcionando
durante quedas de internet.

Quando a conexao voltar, as operacoes feitas localmente devem ser sincronizadas
com a API cloud do Mangefy de forma ordenada, idempotente e auditavel.

---

## Principio de Design

O modo offline deve ser operacional, nao administrativo.

Deve funcionar offline:

- abrir comanda;
- adicionar pedidos;
- enviar pedidos para cozinha;
- atualizar status basico do KDS local;
- fechar comanda;
- registrar pagamento manual;
- imprimir pedidos e comprovantes;
- registrar baixa de estoque local pendente;
- criar pendencia fiscal para emissao posterior;
- sincronizar operacoes quando a internet voltar.

Pode ficar bloqueado offline:

- AdminSaaS;
- troca de plano;
- criacao de tenant;
- criacao ou alteracao de usuarios;
- alteracao de permissoes;
- alteracao de configuracoes criticas;
- integracoes externas, como iFood, Rappi e Google;
- PIX dinamico;
- maquininha integrada;
- emissao fiscal real via hub externo;
- relatorios consolidados ou avancados.

---

## Arquitetura Recomendada

A arquitetura sugerida e criar um **Mangefy Local Agent** instalado no computador
da loja ou caixa principal.

Fluxo conceitual:

```txt
Frontend/PWA
   |
   v
Mangefy Local Agent
   |
   v
Banco local + fila offline
   |
   v
API Cloud Mangefy, quando a internet voltar
   |
   v
PostgreSQL cloud
```

O Local Agent seria responsavel por:

- manter um banco local;
- receber comandos do frontend mesmo sem internet;
- criar uma fila local de operacoes;
- imprimir pedidos e comprovantes;
- detectar queda e retorno de internet;
- sincronizar com a API cloud;
- permitir comunicacao local com cozinha, se houver rede interna.

---

## Banco Local

Para a primeira versao, a recomendacao e usar **SQLite** no Local Agent.

SQLite e suficiente para uma loja individual, simples de distribuir e facil de
manter. PostgreSQL local pode ser considerado em instalacoes maiores ou com
necessidade de maior robustez.

Tabelas locais sugeridas:

- `LocalTabs`
- `LocalOrders`
- `LocalOrderItems`
- `LocalPayments`
- `LocalPrintJobs`
- `OfflineOperations`
- `SyncState`
- `CachedEmployees`
- `CachedMenus`
- `CachedSettings`

O banco local nao precisa reproduzir todo o modelo cloud. Ele deve armazenar
apenas o necessario para a operacao continuar e para a sincronizacao posterior.

---

## Fila Local de Operacoes

Toda acao offline deve virar uma operacao local pendente.

Modelo conceitual:

```txt
OfflineOperation
- Id
- TenantId
- DeviceId
- ClientCommandId
- OperationType
- PayloadJson
- OccurredAt
- Status: Pending | Synced | Failed | Conflict
- ErrorMessage
```

Tipos de operacao sugeridos:

- `OpenTab`
- `SubmitOrder`
- `StartItemPreparation`
- `MarkItemReady`
- `CloseTab`
- `CancelTab`
- `RegisterPayment`
- `CreatePrintJob`
- `CreateStockMovement`
- `CreateFiscalPendingDocument`

Quando a conexao voltar, o Local Agent envia as operacoes para a API cloud em
ordem.

---

## Idempotencia

O projeto ja possui o conceito de `ClientCommandId`, que deve ser usado como
base para o modo offline.

Cada operacao offline deve ter um identificador unico, preferencialmente:

```txt
DeviceId + ClientCommandId
```

Isso evita duplicidade em casos como:

- usuario clicou duas vezes;
- internet caiu durante sincronizacao;
- agente tentou reenviar a mesma operacao;
- API respondeu, mas o agente nao recebeu a resposta.

A API cloud deve aceitar reenvio seguro das operacoes ja processadas.

---

## Sincronizacao

Endpoints futuros sugeridos:

```txt
POST /api/tenants/{tenantId}/sync/push
GET  /api/tenants/{tenantId}/sync/pull?since=...
```

`sync/push`:

- recebe operacoes offline;
- valida tenant, device e usuario;
- processa em ordem;
- respeita idempotencia;
- retorna status de cada operacao.

`sync/pull`:

- envia atualizacoes da cloud para o banco local;
- atualiza cardapios, precos, permissoes, mesas e configuracoes;
- informa status fiscal;
- informa conflitos ou operacoes rejeitadas.

O Local Agent deve manter um `SyncState`, por exemplo:

```txt
SyncState
- TenantId
- DeviceId
- LastPulledAt
- LastPushedAt
- LastSuccessfulSyncAt
- PendingOperationsCount
```

---

## Login Offline

O login offline deve ser restrito e seguro.

Regras sugeridas:

- permitir login offline apenas para usuarios que ja logaram naquele dispositivo;
- armazenar snapshot local das permissoes;
- exigir PIN local ou senha validada contra hash local;
- definir validade do acesso offline, por exemplo 24h ou 72h;
- permitir modo emergencia para Owner;
- registrar auditoria local para sincronizar depois.

Nao deve ser permitido offline:

- criar funcionario;
- alterar permissao;
- alterar cargo;
- trocar senha;
- alterar configuracoes criticas;
- alterar plano ou dados AdminSaaS.

---

## Permissoes Offline

As permissoes devem ser cacheadas localmente.

Durante o offline, o sistema usa o ultimo snapshot valido.

Quando a internet voltar:

- se o usuario perdeu permissao enquanto estava offline, as operacoes ja feitas
  devem ser sincronizadas com alerta de auditoria;
- operacoes sensiveis podem ir para `Conflict` ou exigir revisao do Owner;
- novas operacoes passam a respeitar o estado atualizado.

---

## Conflitos

A regra principal deve ser: **venda e append-only**.

Em vez de editar historico, o sistema deve registrar novos eventos:

- pedido criado;
- item cancelado;
- pagamento registrado;
- comanda fechada;
- ajuste de estoque criado.

Conflitos provaveis:

- numero de comanda duplicado;
- item de cardapio mudou de preco;
- mesa mudou de status;
- estoque ficou insuficiente;
- funcionario perdeu permissao;
- pagamento offline precisa conciliacao;
- documento fiscal ficou pendente ou rejeitado.

Politicas sugeridas:

- aceitar preco do momento offline como snapshot da venda;
- usar `ClientCommandId` para evitar duplicidade;
- deixar conflitos visiveis em uma tela de sincronizacao;
- nao bloquear venda ja feita, exceto casos muito criticos;
- exigir acao do gerente para conflitos fiscais, pagamentos divergentes ou
  permissoes sensiveis.

---

## Numeracao de Comandas Offline

Para reduzir conflito de numeracao, existem duas opcoes.

Opcao 1: faixas por dispositivo:

```txt
Caixa principal: 1-300
Tablet garcom A: 301-600
Tablet garcom B: 601-900
```

Opcao 2: identificador interno global:

- usar `ClientCommandId`/UUID como identidade real;
- manter o numero fisico da comanda apenas como referencia operacional;
- resolver conflito de numero na sincronizacao, se necessario.

Para uma primeira versao, a opcao de faixas por dispositivo e mais simples para
operacao de loja.

---

## Impressao Offline

O Local Agent deve controlar impressao local.

Fluxo sugerido:

```txt
SubmitOrder
   |
   v
CreatePrintJob local
   |
   v
Imprime na cozinha/caixa
   |
   v
Marca como impresso localmente
   |
   v
Sincroniza PrintJob com cloud depois
```

Beneficios:

- pedidos continuam chegando na cozinha;
- comprovantes podem ser emitidos;
- o funcionario nao depende do navegador falar diretamente com a impressora;
- futuramente permite QZ Tray, PrintNode ou agente proprio.

---

## KDS em Rede Local

Se a internet cair, mas a rede interna da loja continuar funcionando, o KDS pode
funcionar via Local Agent.

Fluxo:

```txt
Caixa/garcom
   |
   v
Local Agent
   |
   v
Tela da cozinha na rede local
```

O KDS local pode permitir:

- receber pedidos;
- marcar item em preparo;
- marcar item pronto;
- registrar devolucao simples;
- sincronizar status depois.

SignalR cloud pode continuar sendo o caminho principal quando a internet estiver
disponivel. Offline, o agente local assume.

---

## Pagamentos Offline

Devem ser permitidos offline apenas pagamentos que possam ser registrados sem
confirmacao externa.

Permitidos:

- dinheiro;
- cartao registrado manualmente;
- PIX manual confirmado pelo operador;
- cortesia autorizada por perfil;
- pagamento externo informado manualmente.

Bloqueados ou pendentes:

- PIX dinamico;
- maquininha integrada;
- conciliacao automatica;
- captura online de pagamento;
- estorno integrado.

Pagamentos offline devem ser sincronizados com status de conciliacao:

```txt
ManualConfirmed | PendingConciliation | Rejected | Adjusted
```

---

## Fiscal Offline

Se a emissao fiscal depender de hub externo, o modo offline nao deve prometer
emissao real imediata.

Fluxo sugerido:

```txt
Comanda fechada offline
   |
   v
FiscalDocument = Pending ou Contingency
   |
   v
Internet volta
   |
   v
Sincroniza venda
   |
   v
Emite NFC-e via hub fiscal
   |
   v
Atualiza FiscalDocument
```

Pontos a pesquisar antes de implementar:

- regras de contingencia NFC-e por estado;
- prazo de transmissao apos contingencia;
- mensagens obrigatorias no comprovante;
- numeracao fiscal em contingencia;
- rejeicoes e reemissao.

---

## Tela de Sincronizacao

Quando a internet voltar, o sistema deve exibir uma central de sincronizacao.

Informacoes importantes:

- operacoes pendentes;
- operacoes sincronizadas;
- operacoes com erro;
- conflitos;
- documentos fiscais pendentes;
- pagamentos pendentes de conciliacao;
- ultimo horario de sincronizacao;
- dispositivo que originou cada operacao.

Essa tela reduz inseguranca operacional e ajuda o gerente a entender se tudo foi
enviado corretamente.

---

## Fases de Implementacao

### Fase 1 - Base Offline

- modelar `Device`, `OfflineOperation` e `SyncState`;
- garantir `ClientCommandId` em todos os comandos criticos;
- criar endpoint `sync/push`;
- definir payloads canonicos para operacoes offline;
- registrar auditoria de sincronizacao.

### Fase 2 - Local Agent

- criar servico local;
- adicionar SQLite;
- implementar fila offline;
- implementar health check de internet;
- implementar impressao local basica;
- permitir operacao local de comandas e pedidos.

### Fase 3 - Frontend/PWA

- detectar status offline;
- direcionar operacoes para Local Agent quando disponivel;
- exibir indicador claro de modo offline;
- mostrar contador de operacoes pendentes;
- bloquear telas administrativas offline.

### Fase 4 - Conflitos e Auditoria

- criar tela de sincronizacao;
- criar regras de retry;
- criar status `Failed` e `Conflict`;
- permitir revisao pelo Owner/gerente;
- registrar logs detalhados.

### Fase 5 - Fiscal e KDS Local

- implementar fiscal pendente/contingencia;
- pesquisar regras fiscais brasileiras;
- implementar KDS local;
- melhorar impressao por estacao;
- estudar QZ Tray, PrintNode ou agente proprio de impressao.

---

## Decisao Recomendada

Nome da feature futura:

```txt
Modo Continuidade Offline
```

Definicao:

> Permite que a loja continue operando localmente durante queda de internet,
> registrando vendas, pedidos, pagamentos manuais e impressoes em uma fila
> local, com sincronizacao automatica quando a conexao for restabelecida.

---

## Criterios de Aceite Futuros

- Loja consegue abrir comanda sem internet.
- Loja consegue adicionar pedido sem internet.
- Cozinha recebe pedido via impressao local ou KDS local.
- Loja consegue fechar comanda e registrar pagamento manual.
- Operacoes offline aparecem como pendentes de sincronizacao.
- Ao voltar a internet, operacoes sao enviadas para a API cloud.
- Reenvio da mesma operacao nao gera duplicidade.
- Conflitos ficam visiveis para o gerente.
- Documento fiscal fica pendente/contingencia, sem perda da venda.
- Acoes administrativas sensiveis ficam bloqueadas offline.

---

## Perguntas Abertas

- O Local Agent sera obrigatorio ou opcional?
- O banco local sera SQLite ou PostgreSQL local?
- Qual o tempo maximo permitido de operacao offline?
- O Owner pode liberar operacao offline emergencial sem login recente?
- Como dividir faixas de comandas por dispositivo?
- Como tratar fiscal em contingencia por estado?
- Como conciliar pagamentos manuais feitos offline?
- O KDS offline usara rede local, impressao, ou ambos?
- Qual estrategia de instalacao e atualizacao do Local Agent?

---

## Fora do Escopo Atual

- Implementacao do Local Agent.
- Frontend/PWA offline.
- Endpoints de sincronizacao.
- Banco local.
- Fiscal em contingencia.
- KDS local.
- Impressao offline real.
- Tela de sincronizacao.

