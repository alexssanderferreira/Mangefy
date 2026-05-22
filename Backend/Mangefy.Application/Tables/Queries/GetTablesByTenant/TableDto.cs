using Mangefy.Domain.Tables;

namespace Mangefy.Application.Tables.Queries.GetTablesByTenant;

public sealed record TableDto(
    Guid Id,
    string Number,
    int Capacity,
    string Status,
    string? Section
)
{
    public static TableDto FromDomain(Table t) => new(
        t.Id,
        t.Number,
        t.Capacity,
        t.Status.ToString(),
        t.Section);
}
