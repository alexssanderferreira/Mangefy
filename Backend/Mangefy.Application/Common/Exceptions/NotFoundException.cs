namespace Mangefy.Application.Common.Exceptions;

public sealed class NotFoundException : Exception
{
    public NotFoundException(string entity, object key)
        : base($"{entity} '{key}' não encontrado.") { }
}
