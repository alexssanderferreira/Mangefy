using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Audit;
using Mangefy.Domain.PrintJobs;
using Mangefy.Domain.PrintJobs.Repositories;
using MediatR;

namespace Mangefy.Application.PrintJobs.Commands.ReprintJob;

public sealed class ReprintJobCommandHandler : IRequestHandler<ReprintJobCommand, Guid>
{
    private readonly IPrintJobRepository _jobs;
    private readonly IAuditService _audit;
    private readonly IUnitOfWork _uow;

    public ReprintJobCommandHandler(IPrintJobRepository jobs, IAuditService audit, IUnitOfWork uow)
    {
        _jobs = jobs;
        _audit = audit;
        _uow = uow;
    }

    public async Task<Guid> Handle(ReprintJobCommand request, CancellationToken cancellationToken)
    {
        var job = PrintJob.Reprint(
            request.TenantId,
            request.Station,
            request.Payload,
            request.Reason,
            request.EmployeeId,
            request.PrinterId);

        await _jobs.AddAsync(job, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync(
            request.TenantId,
            request.EmployeeId,
            isAdminSaas: false,
            AuditAction.PrintJobReprinted,
            nameof(PrintJob),
            job.Id,
            reason: request.Reason,
            ct: cancellationToken);

        return job.Id;
    }
}
