using MediatR;

namespace Mangefy.Application.Platform.BusinessTypes.Queries.GetBusinessTypes;

public sealed record GetBusinessTypesQuery : IRequest<IReadOnlyList<BusinessTypeDto>>;

public sealed record RoleTemplateDto(Guid Id, string Name, string? Description, bool IsActive, IReadOnlyCollection<string> Permissions, int UsageCount);

public sealed record BusinessTypeDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    IReadOnlyList<RoleTemplateDto> RoleTemplates,
    int TenantCount);
