using Mangefy.Domain.OperationalSessions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mangefy.Infrastructure.Persistence.Configurations;

public sealed class OperationalSessionConfiguration : IEntityTypeConfiguration<OperationalSession>
{
    public void Configure(EntityTypeBuilder<OperationalSession> builder)
    {
        builder.ToTable("OperationalSessions");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.TenantId, x.EmployeeId, x.Status });

        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
    }
}
