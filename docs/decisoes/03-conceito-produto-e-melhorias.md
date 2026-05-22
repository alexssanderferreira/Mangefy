# Conceito do Produto e Melhorias

Este documento registra a conversa de avaliacao conceitual do Mangefy e as decisoes tomadas sobre escopo, melhorias e direcao do produto.

## Entendimento do Produto

O Mangefy e uma plataforma SaaS para gestao de estabelecimentos de alimentacao, como restaurantes, bares, lanchonetes e negocios similares.

O produto deve atender tres perspectivas principais:

- Painel SaaS: gestao de planos, tenants, cobranca, recursos e administracao da plataforma.
- Painel do Dono/Gerente: configuracao do restaurante, funcionarios, permissoes, cardapio, estoque e relatorios.
- Operacao de Loja: comandas, pedidos, cozinha/KDS, pagamentos, caixa e operacao em tempo real.

A decisao conceitual mais importante e que o nucleo operacional do produto e a comanda individual, nao a mesa.

## Modelo Operacional

- Mesa: representa o espaco fisico e pode agrupar varias comandas abertas.
- Comanda: representa a unidade real de consumo, cobranca, pagamento e auditoria.
- Pedido: representa uma solicitacao feita por uma comanda.
- Item do pedido: representa a unidade operacional acompanhada pela cozinha/KDS.
- Pagamento: fica vinculado a comanda.
- Fechamento de mesa: deve existir apenas como visao operacional, nao como unidade financeira obrigatoria.

Essa abordagem elimina a necessidade de divisao de conta por pessoa, pois cada pessoa pode ter sua propria comanda.

## Pedidos

O dominio deve evoluir para suportar pedidos mais completos:

- adicionais e modificadores;
- observacoes por item;
- combos;
- cancelamento parcial;
- desconto por item;
- taxa de servico;
- status por item;
- item retornado ou refeito;
- motivo de cancelamento.

Uma regra importante: alterar um item antes de enviar para a cozinha e diferente de cancelar um item depois de enviado. Depois que o item entra no fluxo da cozinha, a alteracao deve gerar auditoria.

## Pagamentos

Como a comanda e individual, nao ha necessidade de split por pessoa. Ainda assim, uma comanda deve permitir multiplos pagamentos.

O pagamento deve prever:

- dinheiro;
- pix;
- debito;
- credito;
- voucher;
- cortesia;
- pagamento parcial;
- troco;
- gorjeta;
- taxa de servico;
- desconto;
- fechamento apenas quando o saldo estiver quitado ou dentro de tolerancia configurada.

Toda alteracao financeira relevante deve gerar auditoria.

## Delivery e Retirada

Delivery e retirada nao devem ser tratados como outro sistema separado. A sugestao e modelar como canal de venda.

Possiveis canais:

- salao;
- balcao;
- retirada;
- delivery.

No salao, o pedido pertence a uma comanda. Em balcao, retirada e delivery, o pedido pode pertencer a uma comanda avulsa ou a uma venda direta.

Delivery deve adicionar informacoes especificas, como endereco, taxa de entrega, status de entrega e entregador.

## KDS e Cozinha

O KDS deve ser pensado como fila de producao por praca.

Exemplos de praca:

- cozinha;
- bar;
- sobremesa;
- chapa;
- sushi;
- cafeteria.

Cada item do cardapio deve indicar sua praca de preparo. Ao enviar um pedido, os itens entram nas filas correspondentes.

Estados sugeridos:

- aguardando;
- em preparo;
- pronto;
- entregue;
- cancelado;
- retornado/refazer.

Tambem deve ser previsto:

- prioridade;
- tempo estimado;
- horario de entrada na fila;
- responsavel pela alteracao;
- reimpressao;
- motivo de retorno.

## Fiscal

Fiscal/NF ficara para implementacao futura, mas o dominio deve ser preparado desde cedo.

O fiscal nao deve travar o nucleo da comanda. A recomendacao e criar um conceito separado, como FiscalDocument, gerado a partir de uma venda/comanda fechada.

O dominio deve prever:

- documento fiscal vinculado a venda ou comanda;
- status fiscal: pendente, emitido, rejeitado, cancelado, contingencia;
- chave de acesso;
- numero e serie;
- protocolo de autorizacao;
- XML;
- motivo de rejeicao;
- tipo de documento: NFC-e, SAT e possivelmente NF-e no futuro;
- ambiente: homologacao ou producao.

Assim, a operacao pode fechar uma comanda e o fiscal pode emitir, rejeitar, cancelar ou entrar em contingencia em um fluxo proprio.

## Relatorios e BI

Os relatorios devem ser separados em duas categorias.

Relatorios operacionais:

- vendas do dia;
- caixa atual;
- comandas abertas;
- pedidos atrasados;
- estoque baixo.

Relatorios gerenciais:

- faturamento por periodo;
- ticket medio;
- itens mais vendidos;
- horarios de pico;
- vendas por funcionario;
- cancelamentos;
- descontos;
- CMV;
- margem por produto;
- perdas de estoque.

## Auditoria

Auditoria e essencial para restaurante, especialmente em operacoes financeiras e de estoque.

Eventos que devem ser auditados:

- quem cancelou pedido;
- quem cancelou item;
- quem aplicou desconto;
- quem abriu caixa;
- quem fechou caixa;
- quem realizou retirada;
- quem alterou estoque;
- quem concedeu acesso temporario;
- quem alterou permissoes;
- quem alterou configuracoes criticas.

## Onboarding do Tenant

O onboarding deve criar:

- estabelecimento;
- plano;
- dono;
- cargos padrao;
- permissoes padrao;
- configuracoes basicas.

Mesas iniciais e cardapio inicial ficam sob responsabilidade do dono do restaurante.

## Feature Flags por Plano

O sistema deve diferenciar permissoes de usuario e recursos disponiveis por plano.

Permissoes dizem o que um usuario pode fazer dentro de um tenant.

Feature flags por plano dizem quais modulos ou capacidades aquele tenant pode usar.

Exemplos:

- limite de funcionarios;
- limite de mesas;
- limite de comandas abertas;
- acesso a KDS;
- acesso a delivery;
- acesso a estoque avancado;
- acesso a relatorios avancados;
- acesso a integracoes.

## Experiencia Offline e Instabilidade

Offline total nao deve ser prometido no MVP. A abordagem recomendada e evolutiva.

Para o MVP:

- detectar perda de conexao;
- exibir estado claro de sincronizacao;
- manter cache de cardapio, mesas e comandas abertas;
- permitir leitura durante instabilidade;
- bloquear acoes criticas que possam causar inconsistencias.

Para uma versao futura:

- app local/PWA com fila de comandos offline;
- cada acao vira um evento local;
- sincronizacao quando a internet voltar;
- cada evento deve ter clientId, deviceId, occurredAt e sequenceNumber;
- servidor deve rejeitar eventos duplicados e lidar com conflitos.

Pagamentos, caixa e fiscal exigem cuidado especial e nao devem ser liberados offline sem regras fortes de consistencia.

## Billing SaaS

Billing SaaS representa o modelo de cobranca da plataforma.

O dominio deve prever:

- plano;
- assinatura;
- ciclo de cobranca;
- fatura;
- status da assinatura;
- trial;
- inadimplencia;
- periodo de carencia;
- bloqueio parcial;
- limites por plano.

Inadimplencia nao deve derrubar o restaurante de forma brusca durante a operacao. A recomendacao e usar grace period e bloqueios graduais.

## Estoque Avancado

Estoque basico controla quantidade atual. Estoque avancado conecta cardapio, insumos e custo.

O dominio deve evoluir para:

- item de estoque;
- unidade de medida;
- conversao de unidade;
- ficha tecnica/receita;
- baixa automatica por venda;
- perda/quebra;
- inventario;
- compra;
- fornecedor;
- custo medio;
- CMV;
- alerta de estoque minimo.

Exemplo: vender um hamburguer deve baixar pao, carne, queijo, embalagem e demais ingredientes conforme ficha tecnica.

## Integracoes Futuras

As seguintes integracoes ficam para implementacao futura:

- iFood;
- WhatsApp;
- impressoras;
- TEF;
- fiscal;
- outros canais externos.

O dominio deve evitar acoplamento forte para permitir essas integracoes posteriormente.

## Direcao Recomendada

O Mangefy deve ser priorizado como um sistema de operacao em tempo real, nao apenas como cadastro administrativo.

O fluxo operacional MVP recomendado e:

1. Comanda;
2. Pedido;
3. KDS/cozinha;
4. Pagamento;
5. Caixa;
6. Baixa simples de estoque;
7. Auditoria minima.

Depois disso, podem ser aprofundados:

- billing SaaS;
- estoque avancado;
- fiscal;
- delivery;
- BI/relatorios;
- integracoes externas.

## Decisao Central

A comanda individual e o nucleo operacional do Mangefy.

Mesa e contexto fisico. Comanda e contexto financeiro, operacional e auditavel.
