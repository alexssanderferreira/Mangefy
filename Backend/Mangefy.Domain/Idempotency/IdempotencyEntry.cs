using Mangefy.Domain.Common;

namespace Mangefy.Domain.Idempotency;

/// <summary>
/// Registro de comando já processado. Evita duplicidade em cenários de retry,
/// instabilidade de rede e duplo clique.
/// Escopado por TenantId + CommandId (ClientCommandId gerado pelo cliente).
/// </summary>
public sealed class IdempotencyEntry : Entity
{
    public Guid TenantId { get; private set; }
    public Guid CommandId { get; private set; }
    public string CommandName { get; private set; } = string.Empty;
    public string? ResponseJson { get; private set; }
    public DateTime ExpiresAt { get; private set; }

    private IdempotencyEntry() { }

    public static IdempotencyEntry Create(
        Guid tenantId,
        Guid commandId,
        string commandName,
        string? responseJson,
        TimeSpan? ttl = null)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId inválido.");

        if (commandId == Guid.Empty)
            throw new DomainException("CommandId inválido.");

        if (string.IsNullOrWhiteSpace(commandName))
            throw new DomainException("CommandName não pode ser vazio.");

        return new IdempotencyEntry
        {
            TenantId = tenantId,
            CommandId = commandId,
            CommandName = commandName,
            ResponseJson = responseJson,
            ExpiresAt = DateTime.UtcNow.Add(ttl ?? TimeSpan.FromHours(24))
        };
    }

    public bool IsExpired() => DateTime.UtcNow > ExpiresAt;
}
