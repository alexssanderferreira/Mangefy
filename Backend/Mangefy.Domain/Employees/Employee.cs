using Mangefy.Domain.Common;
using Mangefy.Domain.Common.ValueObjects;
using Mangefy.Domain.Employees.Events;

namespace Mangefy.Domain.Employees;

public sealed class Employee : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; }
    public Email Email { get; private set; }
    public string? PasswordHash { get; private set; }
    public Guid TenantRoleId { get; private set; }
    public EmployeeStatus Status { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    /// <summary>
    /// Acesso temporário fora do turno concedido pelo Owner.
    /// Nulo significa que não há liberação ativa.
    /// </summary>
    public DateTime? TemporaryAccessUntil { get; private set; }
    public uint RowVersion { get; private set; }

    private Employee() { }

    /// <summary>
    /// Cria um funcionário vinculado a um cargo (TenantRole) existente.
    /// Senha é definida posteriormente via token de ativação (status PendingActivation).
    /// </summary>
    public static Employee Create(Guid tenantId, string name, string email, Guid tenantRoleId)
    {
        var employee = Build(tenantId, name, email, tenantRoleId);
        employee.Status = EmployeeStatus.PendingActivation;
        return employee;
    }

    private static Employee Build(Guid tenantId, string name, string email, Guid roleId)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId inválido.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do funcionário não pode ser vazio.");

        if (roleId == Guid.Empty)
            throw new DomainException("Cargo inválido.");

        var employee = new Employee
        {
            TenantId = tenantId,
            Name = name.Trim(),
            Email = Email.Create(email),
            TenantRoleId = roleId
        };

        employee.AddDomainEvent(new EmployeeCreatedEvent(employee.Id, tenantId, employee.Email.Value));
        return employee;
    }

    public void Activate()
    {
        if (Status == EmployeeStatus.Inactive)
            throw new DomainException("Funcionário inativo não pode ser ativado diretamente. Use Reactivate.");

        Status = EmployeeStatus.Active;
        SetUpdatedAt();
    }

    public void Reactivate()
    {
        if (Status != EmployeeStatus.Inactive)
            throw new DomainException("Apenas funcionários inativos podem ser reativados.");

        Status = EmployeeStatus.Active;
        SetUpdatedAt();
    }

    public void Deactivate()
    {
        Status = EmployeeStatus.Inactive;
        SetUpdatedAt();
    }

    public void UpdateProfile(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome não pode ser vazio.");

        Name = name.Trim();
        SetUpdatedAt();
    }

    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new DomainException("Hash da senha não pode ser vazio.");

        PasswordHash = newPasswordHash;
        SetUpdatedAt();
    }

    public void AssignRole(Guid newRoleId)
    {
        if (newRoleId == Guid.Empty)
            throw new DomainException("Cargo inválido.");

        TenantRoleId = newRoleId;
        SetUpdatedAt();
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    /// <summary>
    /// Concede acesso temporário fora do turno. Chamado pelo Owner.
    /// </summary>
    public void GrantTemporaryAccess(DateTime until)
    {
        if (until <= DateTime.UtcNow)
            throw new DomainException("A data de término do acesso temporário deve ser no futuro.");

        TemporaryAccessUntil = until;
        SetUpdatedAt();
    }

    public void RevokeTemporaryAccess()
    {
        TemporaryAccessUntil = null;
        SetUpdatedAt();
    }

    public bool HasTemporaryAccess() =>
        TemporaryAccessUntil.HasValue && TemporaryAccessUntil.Value > DateTime.UtcNow;
}
