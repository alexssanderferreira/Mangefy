using MediatR;

namespace Mangefy.Application.Tables.Commands.UpdateTable;

public sealed record UpdateTableCommand(
    Guid TenantId,
    Guid TableId,
    string Number,
    int Capacity,
    string? Section) : IRequest;
