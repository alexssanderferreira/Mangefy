using Mangefy.Domain.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mangefy.Infrastructure.Persistence.Configurations;

public sealed class TableConfiguration : IEntityTypeConfiguration<Table>
{
    public void Configure(EntityTypeBuilder<Table> builder)
    {
        builder.ToTable("Tables");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Number).IsRequired().HasMaxLength(20);
        builder.HasIndex(x => new { x.TenantId, x.Number }).IsUnique();
        builder.Property(x => x.Section).HasMaxLength(100);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
    }
}
