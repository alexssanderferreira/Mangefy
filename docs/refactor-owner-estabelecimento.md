# Refactor: Separação Owner / Estabelecimento

## Objetivo
Separar o conceito de **Dono (Owner)** do conceito de **Estabelecimento (Tenant)**.
O Owner é uma entidade de plataforma criada pelo Admin SaaS.
O Tenant pertence a um Owner e só pode ser criado/desativado pelo Admin SaaS.

---

## Etapas

### Domínio
- [x] 1. Criar entidade `Owner` (Id, Name, Email, PasswordHash, Status, CreatedAt)
- [x] 2. Criar `IOwnerRepository`
- [x] 3. Mover `ActivationToken` para pertencer ao `Owner` (`OwnerActivationToken`)
- [x] 4. Adicionar `OwnerId` em `Tenant`, tornar `Email` nullable
- [x] 5. Adicionar `MaxEstablishments` em `Plan`
- [x] 6. Remover `TenantRole.CreateOwnerRole` e `Employee.CreateOwner` / `Employee.IsOwner`

### Infraestrutura
- [x] 7. Criar configuração EF para `Owner` e `OwnerActivationToken`
- [x] 8. Implementar `OwnerRepository` e `OwnerActivationTokenRepository`
- [x] 9. Criar migration de schema (tabela `Owners`, FK em `Tenants`, coluna em `Plans`, drop `IsOwner`)
- [x] 10. Migration de dados: converter employees com role IsOwnerRole em registros `Owner`, linkar `Tenant.OwnerId`

### Aplicação
- [x] 11. `CreateOwnerCommand` — Admin SaaS cria owner (nome, email); gera `OwnerActivationToken` de 48h
- [x] 12. `GetOwnerByIdQuery` (retorna `OwnerDetailDto` com lista de tenants) / `ListOwnersQuery` (paginado)
- [x] 13. `ActivateOwnerCommand` / `DeactivateOwnerCommand`
- [x] 14. Refatorar `CreateTenantCommand` — recebe `OwnerId`, valida limite do plano, não cria employee owner
- [x] 15. Refatorar `ResolveTenantsCommand` — consulta via `Owner` por email/senha
- [x] 16. Refatorar `LoginCommandHandler` — usa `role.IsOwnerRole` em vez de `employee.IsOwner`
- [x] 17. Refatorar `SwitchTenantCommandHandler` — usa `role.IsOwnerRole` em vez de `targetEmployee.IsOwner`

### API
- [x] 18. Criar `OwnersController` (`GET /api/admin/owners`, `GET /{id}`, `POST`, `PATCH /{id}/activate`, `PATCH /{id}/deactivate`) — requer `[RequireAdminSaas]`
- [x] 19. Atualizar `TenantsController.Create` — recebe `OwnerId` em vez de dados do owner; removido `check-owner-email`
- [x] 20. `AuthController` — `ResolveTenantsCommand` já atualizado para usar `Owner`

### Frontend — Painel Admin
- [x] 21. Criar serviço `OwnerService` (DTOs + métodos HTTP)
- [x] 22. Criar tela **Donos** (listagem com busca, paginação e drawer de criação)
- [x] 23. Criar tela de detalhe do Owner (info, status, último acesso, lista de estabelecimentos, ativar/desativar)
- [x] 24. Criar drawer de criação de Owner (nome + e-mail; info sobre link de ativação de 48h)
- [x] 25. Atualizar rotas admin (`/owners`, `/owners/:id`) e sidebar com link "Donos"
- [x] 26. Refatorar formulário de criação de Tenant — campo owner vira busca/select de owner existente
- [x] 27. Remover lógica `check-owner-email` e modal de confirmação de e-mail duplicado

### Frontend — Auth
- [x] 28. Ajustar fluxo de login Owner vs Employee
  - `LoginCommandHandler`: tenta Employee primeiro; fallback para Owner (verifica `Tenant.OwnerId == owner.Id`)
  - `SwitchTenantCommandHandler`: detecta sessão de Owner via `ICurrentUser.OwnerId`
  - JWT: novo `GenerateOwnerTenantToken` com claim `ownerId`
  - `ICurrentUser.OwnerId` + `HttpContextCurrentUser` lê claim `ownerId`
  - Frontend: `LoginApiResponse` e `CurrentUser` com `ownerId: string | null`

---

## Observações
- Employees continuam existindo dentro do tenant como funcionários normais
- O Owner não aparece como Employee no tenant
- A role `Owner` dentro do tenant pode ser eliminada ou renomeada para `Gerente Geral`
- O limite `MaxEstablishments` do plano é validado no backend ao criar tenant
