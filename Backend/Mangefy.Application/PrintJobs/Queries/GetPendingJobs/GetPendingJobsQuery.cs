using MediatR;

namespace Mangefy.Application.PrintJobs.Queries.GetPendingJobs;

public sealed record GetPendingJobsQuery(Guid TenantId) : IRequest<IReadOnlyList<PrintJobDto>>;

public sealed record PrintJobDto(
    Guid Id,
    string Type,
    string Station,
    string Status,
    int Attempts,
    DateTime CreatedAt);
