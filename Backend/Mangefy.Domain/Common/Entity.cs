namespace Mangefy.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }

    /// <summary>
    /// Funcionário que criou o registro. Nulo para entidades da plataforma (AdminSaas).
    /// </summary>
    public Guid? CreatedByEmployeeId { get; protected set; }

    /// <summary>
    /// Último funcionário que modificou o registro. Nulo para entidades da plataforma.
    /// </summary>
    public Guid? UpdatedByEmployeeId { get; protected set; }

    protected Entity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    protected void SetUpdatedAt()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetCreatedByEmployee(Guid employeeId)
    {
        CreatedByEmployeeId = employeeId;
    }

    public void SetUpdatedByEmployee(Guid employeeId)
    {
        UpdatedByEmployeeId = employeeId;
        UpdatedAt ??= DateTime.UtcNow;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();
}
