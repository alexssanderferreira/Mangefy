using MediatR;

namespace Mangefy.Application.Tables.Commands.SetTableStatus;

/// <summary>
/// Permite ao Owner marcar uma mesa como Unavailable (em manutenção) ou Available novamente.
/// Status Occupied e Reserved são gerenciados automaticamente pelo sistema via domain events.
/// </summary>
public sealed record SetTableStatusCommand(Guid TenantId, Guid TableId, string Status) : IRequest;
