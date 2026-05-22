using Mangefy.Domain.Common;

namespace Mangefy.Domain.Owners;

public sealed class OwnerActivationToken : Entity
{
    public Guid OwnerId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public bool IsUsed { get; private set; }

    private OwnerActivationToken() { }

    public static OwnerActivationToken Create(Guid ownerId, TimeSpan validFor)
    {
        if (ownerId == Guid.Empty)
            throw new DomainException("OwnerId inválido para token de ativação.");

        return new OwnerActivationToken
        {
            OwnerId = ownerId,
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
