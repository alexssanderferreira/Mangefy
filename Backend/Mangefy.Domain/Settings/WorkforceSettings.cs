using Mangefy.Domain.Common;

namespace Mangefy.Domain.Settings;

/// <summary>
/// Configurações de gestão de força de trabalho do tenant.
/// Define tolerância após fim de turno e outras regras operacionais de acesso.
/// </summary>
public sealed class WorkforceSettings : AggregateRoot
{
    public Guid TenantId { get; private set; }

    /// <summary>
    /// Minutos de tolerância após o fim do turno antes de bloquear o acesso do funcionário.
    /// Configurado pelo Owner na tela de configurações.
    /// </summary>
    public int ShiftToleranceMinutes { get; private set; }

    private WorkforceSettings() { }

    public static WorkforceSettings CreateDefault(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId inválido.");

        return new WorkforceSettings
        {
            TenantId = tenantId,
            ShiftToleranceMinutes = 15
        };
    }

    public void UpdateShiftTolerance(int minutes)
    {
        if (minutes < 0)
            throw new DomainException("Período de tolerância não pode ser negativo.");

        if (minutes > 480)
            throw new DomainException("Período de tolerância não pode exceder 8 horas.");

        ShiftToleranceMinutes = minutes;
        SetUpdatedAt();
    }
}
