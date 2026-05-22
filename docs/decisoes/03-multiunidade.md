# Decisão Arquitetural — Multiunidade

## Decisão Atual

**Tenant = uma única unidade física (restaurante, bar, padaria etc.).**

Cada tenant criado pelo AdminSaas representa exatamente um estabelecimento. Não há nenhum nível intermediário de agrupamento.

Esta é a decisão vigente e não será alterada no ciclo atual.

---

## Justificativa

- Simplicidade operacional: toda a lógica de comanda, caixa, estoque, cardápio e funcionários é escopada por `TenantId` sem ambiguidade.
- Billing direto: cada `Subscription` pertence a um tenant → uma unidade → uma fatura.
- Multitenancy já implementado de forma limpa (JWT com `tenantId`, `ValidateTenantAccess`, repositórios filtrados por `TenantId`).
- A maioria dos clientes alvo (restaurantes independentes, bares, padarias) opera uma única unidade.

---

## Opção Futura — Empresa + Unidades (Store/Branch)

Quando um cliente precisar gerenciar múltiplas unidades (ex.: rede de franquias, grupo de restaurantes), a abordagem recomendada é:

```
Organization (Empresa)
└── Store / Branch (Unidade Física)
    └── [operações atuais de Tenant]
```

O `Tenant` atual passaria a representar uma `Store/Branch`, e seria criado um nível `Organization` acima, responsável por:
- Agregação de relatórios entre unidades.
- Gestão centralizada de funcionários (com permissões por unidade).
- Cardápio compartilhado com variações por unidade.
- Assinatura unificada por empresa (não por unidade).

---

## Impacto Estimado por Módulo

| Módulo | Impacto |
|--------|---------|
| **Estoque** | Cada unidade teria seu próprio `Stock`. Transferência entre unidades seria um novo fluxo. |
| **Caixa** | Fechamento por unidade, consolidação por empresa seria um novo relatório. |
| **Funcionários** | Employee precisaria de `StoreId` ou lista de unidades permitidas. Turnos por unidade. |
| **Cardápio** | Suporte a "cardápio base da empresa + sobreposição por unidade" (preços regionais). |
| **Relatórios** | Hoje: por tenant. Futuro: por unidade + consolidado por empresa. |
| **Fiscal** | Cada unidade tem CNPJ próprio → cada `Store` precisaria de `FiscalSettings` separado. |
| **Assinaturas** | Hoje: 1 `Subscription` por tenant. Futuro: 1 por empresa, com licenças por unidade. |
| **Dispositivos** | `Device` já tem `TenantId` → migra para `StoreId` sem quebra de conceito. |
| **Audit** | `AuditLog` teria `StoreId` além de `TenantId` para filtros cruzados. |

---

## Como Migrar quando for necessário

1. Introduzir `Organization` aggregate e `Store` aggregate no Domain.
2. Adicionar `OrganizationId` a `Tenant` (renomeado para `Store`).
3. Os `TenantId` atuais nos tokens JWT passam a ser `StoreId`.
4. Relatórios ganham nível de agregação por `OrganizationId`.
5. Billing muda de `Subscription(TenantId)` para `Subscription(OrganizationId, storeCount)`.

**A mudança é uma adição de camada, não uma reescrita do domínio operacional.**  
As entidades de operação (`Tab`, `Stock`, `CashRegister`, etc.) continuam escopadas por um único `TenantId`/`StoreId` sem alteração.

---

## Registro de Decisão

- **Data:** 2026-05-12
- **Decisão:** Manter tenant = unidade única. Não implementar multiunidade agora.
- **Revisão sugerida:** Quando o primeiro cliente solicitar gerenciamento de 2+ unidades.
