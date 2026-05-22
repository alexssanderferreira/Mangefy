using Mangefy.Domain.Menus;
using MediatR;

namespace Mangefy.Application.PrintJobs.Commands.ReprintJob;

public sealed record ReprintJobCommand(
    Guid TenantId,
    Guid EmployeeId,
    MenuItemStation Station,
    string Payload,
    string Reason,
    Guid? PrinterId = null
) : IRequest<Guid>;
