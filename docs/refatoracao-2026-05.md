
# Refatoração — Maio 2026

Plano de revisão e refatoração gerado após auditoria completa do código.
Itens marcados com `[x]` foram concluídos.

---

## Fase 1 — Domínio

- [x] **1.1** `Employee.Reactivate()` ausente — método citado na mensagem de erro mas não existe. Criar `Reactivate()` espelhando `Owner.Reactivate()`.
- [x] **1.2** `Reservation.Cancel(reason)` contamina `Notes` com o motivo. Adicionar campo `CancellationReason` (string?, nullable). Não modificar `Notes`.
- [x] **1.3** `Tab.DiscountAmount`, `ServiceFee` e `Tip` são `decimal` raw enquanto `Total` retorna `Money`. Converter os três campos para `Money` + atualizar `TabConfiguration` + migration.
- [x] **1.4** `FeatureGracePeriod` expirado ainda é carregado do banco (filtro só em memória). Adicionar `.Where(g => g.ExpiresAt > DateTime.UtcNow)` no repositório. *(já estava correto)*
- [x] **1.5** `Plan.Create` aceita `monthlyPrice = 0` sem validação. Definir se plano gratuito é válido; se não, adicionar `if (monthlyPrice < 0)` no `Create`. *(0 é permitido; negativos são bloqueados)*
- [x] **1.6** `StockItem.Station` usa `MenuItemStation` (enum do módulo Menus). Criar `StockStation` enum separado. Atualizar `StockItem`, `StockConfiguration` + migration.
- [x] **1.7** `Entity.SetUpdatedAt()` tem parâmetro `updatedByEmployeeId` que conflita com o `UnitOfWork`. Remover o parâmetro do domínio; responsabilidade fica exclusivamente no `UnitOfWork`.

---

## Fase 2 — Application Layer

- [x] **2.1** `SubmitOrderCommandHandler` faz N queries separadas para itens do cardápio. Adicionar `IMenuRepository.GetItemsByIdsAsync(IEnumerable<Guid>)` e substituir o `foreach` com lookup por 1 query.
- [x] **2.2** `GrantTemporaryAccessCommandHandler` calcula `accessUntil` com `DateTime.UtcNow` ignorando timezone do tenant. Buscar `Tenant.Timezone` e converter com `TimeZoneInfo`.
- [x] **2.3** `CloseTabCommandHandler` tem duplo `if (request.DiscountAmount > 0)` consecutivo. Unificar em um único bloco.
- [x] **2.4** `CreateSubscriptionCommandHandler` não verifica se `TenantId` e `PlanId` existem. Adicionar lookups com `NotFoundException`.
- [x] **2.5** Commands sem validators: `CancelOrderItemCommand`, `ReturnOrderItemCommand`, `StartItemPreparationCommand`, `CancelTabCommand`. Criar validators com `NotEmpty()` em todos os Ids obrigatórios.
- [x] **2.6** `Result<T>` em `Common/Result.cs` não é usado por nenhum handler. Remover o arquivo.

---

## Fase 3 — Infrastructure

- [x] **3.1** `StockMovements` sem índice em `StockItemId`. Adicionar `HasIndex` em `StockConfiguration` + migration.
- [x] **3.2** `RecipeIngredient` sem chave primária explícita no EF. Definir chave composta `(MenuItemId, StockItemId)` na `MenuConfiguration`.
- [x] **3.3** `OrderItem.Modifiers` persistido como string com `|`. Converter para JSON (`jsonb`) + migration.
- [x] **3.4** Segundo `SaveChangesAsync` no `UnitOfWork` não captura `DbUpdateConcurrencyException`. Replicar o `catch` do primeiro save.
- [x] **3.5** Índice único `{TenantId, Email}` em `Employee` adicionado via SQL raw na migration `Phase3_InfraImprovements` (EF Core não suporta `HasIndex` em owned type + owner combinados via lambda).

Migration gerada: `Phase3_InfraImprovements`.

---

## Fase 4 — Seed (banco será dropado e recriado do zero)

- [x] **4.1** Seed não cria `PlanFeatureSet` — feature gates bloqueiam todas as operações. Adicionada matriz Plano × BusinessType com features por tier (Starter básico, Profissional completo, Enterprise tudo).
- [x] **4.2** Seed não cria dados operacionais nos tenants. Adicionado por tenant (não-suspenso): login como owner, 1 cargo Gerente, 1 funcionário Administrador, 5 mesas, 1 cardápio + 1 categoria + 3 itens.
- [x] **4.3** Senha `"Admin@123"` hardcoded no código. Lida de `args[0]` ou env var `MANGEFY_SEED_PASS` com fallback para padrão.

---

## Fase 5 — Documentação

- [x] **5.1** Seção de onboarding em `docs/dominio/01-estrutura-e-agregados.md` descrevia fluxo antigo (Employee do dono). Reescrita: Owner → ActivationToken → Tenant → TenantRoles + nota sobre separação Owner × Employee.
- [x] **5.2** `RowVersion` documentado como `byte[]` em decisões de design, mas implementado como `uint xmin` (PostgreSQL). Corrigido para `uint xmin` com `HasColumnType("xid")`.
- [x] **5.3** Swagger ativo em `Program.cs` com suporte a JWT Bearer. Nenhuma ação necessária.

---

## Resultado das fases 1–5

- Build: **0 erros, 0 avisos**
- Todas as 24 tarefas concluídas
- Migration `Phase3_InfraImprovements` aplicada
- Seed executa com sucesso (10 owners, 10 tenants, 2 suspensos)

---

## Fase 6 — Bug crítico EF Core OwnsMany (EM ANDAMENTO)

**Contexto:** EF Core 8 + PostgreSQL com `OwnsMany` e chaves Guid geradas no cliente.
Ao adicionar uma entidade filha (categoria, item, order, etc.) em um agregado já rastreado,
o `DetectChanges` marca a nova entidade como `Modified` (em vez de `Added`) porque uma chave
Guid não-zero parece uma linha existente. Resultado: `UPDATE` em linha inexistente → 0 rows
→ `DbUpdateConcurrencyException` → HTTP 409.

**Repositórios afetados:** `MenuRepository`, `TabRepository`, `StockRepository`, `CashRegisterRepository`

**Solução adotada (parcialmente verificada):**

Nos quatro repositórios, o `UpdateAsync` foi reescrito para:
1. Desabilitar `AutoDetectChangesEnabled` **antes** do primeiro `Entry()` (evita que `Entry()` dispare `DetectChanges`)
2. Iterar as coleções próprias; para entidades em estado `Detached` → chamar `_context.Add(entity)` (não apenas `State = Added` — o `Add()` atravessa o grafo e inicializa valores de `OwnsOne` como `Price.Amount`)
3. Reabilitar `AutoDetectChangesEnabled` no `finally`

```csharp
_context.ChangeTracker.AutoDetectChangesEnabled = false;
try
{
    if (_context.Entry(aggregate).State == EntityState.Detached)
    {
        _context.Set<T>().Update(aggregate);
        return Task.CompletedTask;
    }
    foreach (var child in aggregate.Children)
    {
        if (_context.Entry(child).State == EntityState.Detached)
            _context.Add(child); // Add() traversa OwnsOne/OwnsMany internos
    }
}
finally { _context.ChangeTracker.AutoDetectChangesEnabled = true; }
```

**Status dos repositórios:**
- `TabRepository` ✅ — padrão aplicado (Detached check para Orders, Items, Payments)
- `StockRepository` ✅ — padrão aplicado (Detached check para Items, Movements)
- `CashRegisterRepository` ✅ — padrão aplicado (Detached check para Withdrawals, Supplies)
- `MenuRepository` ⚠️ — **PENDENTE VERIFICAÇÃO** — última versão usa `_context.Add(cat/item)` para Detached; categoria criação funcionou; item ainda não foi testado após a última alteração

**Próximo passo ao retomar:**

1. Reiniciar a API (`dotnet run --project Backend/Mangefy.API`)
2. Testar criação de item de menu:
```powershell
$login = Invoke-RestMethod -Method POST -Uri "http://localhost:5184/api/auth/login" `
  -ContentType "application/json" `
  -Body '{"tenantSlug":"bom-sabor","email":"contato@bomsabor.com.br","password":"Admin@123"}'
$token = $login.accessToken; $tid = $login.tenantId

$menus = Invoke-RestMethod -Uri "http://localhost:5184/api/tenants/$tid/menus" `
  -Headers @{Authorization="Bearer $token"}
$mid = $menus[0].id

$cat = Invoke-RestMethod -Method POST `
  -Uri "http://localhost:5184/api/tenants/$tid/menus/$mid/categories" `
  -ContentType "application/json" -Headers @{Authorization="Bearer $token"} `
  -Body '{"name":"Pratos","displayOrder":1}'

Invoke-RestMethod -Method POST `
  -Uri "http://localhost:5184/api/tenants/$tid/menus/$mid/categories/$($cat.id)/items" `
  -ContentType "application/json" -Headers @{Authorization="Bearer $token"} `
  -Body '{"name":"Prato do Dia","price":32.90,"requiresKds":false,"station":"Kitchen"}'
```

3. Se item funcionar → rodar seed completo (drop DB → migrations → seed)
4. Se ainda falhar → investigar se `_context.Add(item)` com `AutoDetectChangesEnabled=false` popula corretamente o `OwnsOne Price`

**Outras pendências identificadas:**
- `MenuRepository`: `_context.Add(cat)` tem comentário `// Add() recursa into cat.Items automatically` — verificar se itens já existentes na categoria não são re-inseridos como duplicatas quando se adiciona um segundo item
- Seed: `"Cargo onboarding usado (00000000-0000-0000-0000-000000000000)"` para planos Starter — a query `GET /roles` pode estar retornando lista vazia ou formato inesperado; o funcionário não é criado quando `roleId == Guid.Empty`
- `JsonStringEnumConverter` adicionado ao `Program.cs` — verificar se não quebrou nenhum endpoint que recebia enum como inteiro

---

Para aplicar no banco do zero:
```bash
dotnet ef database drop --project Backend/Mangefy.Infrastructure --startup-project Backend/Mangefy.API --force
dotnet ef database update --project Backend/Mangefy.Infrastructure --startup-project Backend/Mangefy.API
dotnet run --project Backend/Mangefy.Seed
```
