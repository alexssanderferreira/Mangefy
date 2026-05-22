using Mangefy.Domain.PrintJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mangefy.Infrastructure.Persistence.Configurations;

public sealed class PrintJobConfiguration : IEntityTypeConfiguration<PrintJob>
{
    public void Configure(EntityTypeBuilder<PrintJob> builder)
    {
        builder.ToTable("PrintJobs");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.TenantId, x.Status });

        builder.Property(x => x.Payload).IsRequired().HasMaxLength(8000);
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Station).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.ReimpressionReason).HasMaxLength(500);
    }
}
