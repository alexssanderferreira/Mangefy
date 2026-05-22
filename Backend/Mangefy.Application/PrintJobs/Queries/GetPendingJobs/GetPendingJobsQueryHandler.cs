using Mangefy.Domain.PrintJobs.Repositories;
using MediatR;

namespace Mangefy.Application.PrintJobs.Queries.GetPendingJobs;

public sealed class GetPendingJobsQueryHandler : IRequestHandler<GetPendingJobsQuery, IReadOnlyList<PrintJobDto>>
{
    private readonly IPrintJobRepository _jobs;

    public GetPendingJobsQueryHandler(IPrintJobRepository jobs) => _jobs = jobs;

    public async Task<IReadOnlyList<PrintJobDto>> Handle(GetPendingJobsQuery request, CancellationToken cancellationToken)
    {
        var list = await _jobs.GetPendingByTenantAsync(request.TenantId, cancellationToken);
        return list.Select(j => new PrintJobDto(
            j.Id, j.Type.ToString(), j.Station.ToString(), j.Status.ToString(), j.Attempts, j.CreatedAt)).ToList();
    }
}
