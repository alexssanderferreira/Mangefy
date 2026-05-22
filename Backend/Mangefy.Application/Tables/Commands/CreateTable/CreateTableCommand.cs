using MediatR;

namespace Mangefy.Application.Tables.Commands.CreateTable;

public sealed record CreateTableCommand(
    Guid TenantId,
    string Number,
    int Capacity,
    string? Section
) : IRequest<Guid>;
