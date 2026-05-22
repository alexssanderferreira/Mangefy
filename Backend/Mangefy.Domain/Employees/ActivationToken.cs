using Mangefy.Domain.Common;

namespace Mangefy.Domain.Employees;

public sealed class ActivationToken : Entity
{
    public Guid EmployeeId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public bool IsUsed { get; private set; }

    private ActivationToken() { }

    public static ActivationToken Create(Guid employeeId, TimeSpan validFor)
    {
        if (employeeId == Guid.Empty)
            throw new DomainException("EmployeeId inválido para token de ativação.");

        return new ActivationToken
        {
            EmployeeId = employeeId,
            Token = Guid.NewGuid().ToString("N"),
            ExpiresAt = DateTime.UtcNow.Add(validFor),
            IsUsed = false
        };
    }

    public bool IsValid() => !IsUsed && ExpiresAt > DateTime.UtcNow;

    public void MarkAsUsed()
    {
        if (IsUsed)
            throw new DomainException("Token já utilizado.");

        IsUsed = true;
        SetUpdatedAt();
    }
}
