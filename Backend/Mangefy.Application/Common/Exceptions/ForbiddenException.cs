namespace Mangefy.Application.Common.Exceptions;

public sealed class ForbiddenException : Exception
{
    public ForbiddenException(string permission)
        : base($"Acesso negado. Permissão necessária: {permission}") { }

    public ForbiddenException() : base("Acesso negado.") { }
}
