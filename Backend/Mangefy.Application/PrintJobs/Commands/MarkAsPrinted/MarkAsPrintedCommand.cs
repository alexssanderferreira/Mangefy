using MediatR;

namespace Mangefy.Application.PrintJobs.Commands.MarkAsPrinted;

public sealed record MarkAsPrintedCommand(Guid TenantId, Guid PrintJobId) : IRequest;
