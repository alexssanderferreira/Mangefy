using Mangefy.Domain.Common;

namespace Mangefy.Domain.OperationalSessions;

/// <summary>
/// Sessão operacional de um funcionário. Representa o contexto de trabalho ativo:
/// quem está operando, em qual dispositivo e se está dentro do turno.
/// Preparatório para auditoria enriquecida e sincronização offline futura.
/// Não substitui autenticação JWT — é contexto operacional complementar.
/// </summary>
public sealed class OperationalSession : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public Guid? DeviceId { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? EndedAt { get; private set; }
    public OperationalSessionStatus Status { get; private set; }

    /// <summary>
    /// Indica se o funcionário estava dentro do horário de turno ao iniciar a sessão.
    /// Atualizado pela Application layer ao criar a sessão.
    /// </summary>
    public bool IsWithinShift { get; private set; }

    /// <summary>
    /// Indica se o funcionário possui acesso temporário ativo no momento da sessão.
    /// </summary>
    public bool HasTemporaryAccess { get; private set; }

    private OperationalSession() { }

    public static OperationalSession Start(
        Guid tenantId,
        Guid employeeId,
        bool isWithinShift,
        bool hasTemporaryAccess,
        Guid? deviceId = null)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId inválido.");

        if (employeeId == Guid.Empty)
            throw new DomainException("EmployeeId inválido.");

        return new OperationalSession
        {
            TenantId = tenantId,
            EmployeeId = employeeId,
            DeviceId = deviceId,
            StartedAt = DateTime.UtcNow,
            Status = OperationalSessionStatus.Active,
            IsWithinShift = isWithinShift,
            HasTemporaryAccess = hasTemporaryAccess
        };
    }

    public void End()
    {
        if (Status == OperationalSessionStatus.Ended)
            return;

        Status = OperationalSessionStatus.Ended;
        EndedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    public bool IsActive() => Status == OperationalSessionStatus.Active;
}
