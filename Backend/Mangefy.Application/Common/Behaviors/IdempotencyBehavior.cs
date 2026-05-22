using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Idempotency;
using Mangefy.Domain.Idempotency.Repositories;
using MediatR;
using System.Text.Json;

namespace Mangefy.Application.Common.Behaviors;

/// <summary>
/// Idempotência para comandos críticos. Evita duplicidade em retries, instabilidade de rede e duplo clique.
/// Escopado por TenantId + ClientCommandId. TTL padrão: 24h.
/// Comandos que implementam IIdempotentCommand expõem um ClientCommandId gerado pelo cliente.
/// Se o mesmo ClientCommandId já foi processado, retorna o resultado armazenado sem reprocessar.
/// </summary>
public sealed class IdempotencyBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IIdempotencyRepository _repository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _uow;

    public IdempotencyBehavior(
        IIdempotencyRepository repository,
        ICurrentUser currentUser,
        IUnitOfWork uow)
    {
        _repository = repository;
        _currentUser = currentUser;
        _uow = uow;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not IIdempotentCommand idempotent || !idempotent.ClientCommandId.HasValue)
            return await next();

        var commandId = idempotent.ClientCommandId.Value;
        var tenantId = _currentUser.TenantId ?? Guid.Empty;

        if (tenantId == Guid.Empty)
            return await next();

        var existing = await _repository.GetAsync(tenantId, commandId, cancellationToken);

        if (existing is not null && !existing.IsExpired())
        {
            if (existing.ResponseJson is null)
                return default!;

            return JsonSerializer.Deserialize<TResponse>(existing.ResponseJson)!;
        }

        var response = await next();

        var responseJson = response is null ? null : JsonSerializer.Serialize(response);
        var entry = IdempotencyEntry.Create(tenantId, commandId, typeof(TRequest).Name, responseJson);
        await _repository.AddAsync(entry, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return response;
    }
}

/// <summary>
/// Marcador para comandos que suportam idempotência via ClientCommandId gerado pelo cliente.
/// Use para operações críticas: abertura de comanda, envio de pedido, pagamento, fechamento, cancelamento.
/// A chave é escopada por TenantId e tem TTL de 24h.
/// </summary>
public interface IIdempotentCommand
{
    Guid? ClientCommandId { get; }
}
