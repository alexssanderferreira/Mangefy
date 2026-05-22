using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.PrintJobs.Repositories;
using MediatR;

namespace Mangefy.Application.PrintJobs.Commands.MarkAsPrinted;

public sealed class MarkAsPrintedCommandHandler : IRequestHandler<MarkAsPrintedCommand>
{
    private readonly IPrintJobRepository _jobs;
    private readonly IUnitOfWork _uow;

    public MarkAsPrintedCommandHandler(IPrintJobRepository jobs, IUnitOfWork uow)
    {
        _jobs = jobs;
        _uow = uow;
    }

    public async Task Handle(MarkAsPrintedCommand request, CancellationToken cancellationToken)
    {
        var job = await _jobs.GetByIdAsync(request.PrintJobId, cancellationToken)
            ?? throw new NotFoundException("PrintJob", request.PrintJobId);

        if (job.TenantId != request.TenantId)
            throw new ForbiddenException();

        job.MarkAsPrinted();
        await _jobs.UpdateAsync(job, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
