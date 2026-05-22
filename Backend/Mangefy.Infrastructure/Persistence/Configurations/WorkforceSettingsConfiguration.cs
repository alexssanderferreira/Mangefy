using Mangefy.Domain.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mangefy.Infrastructure.Persistence.Configurations;

public sealed class WorkforceSettingsConfiguration : IEntityTypeConfiguration<WorkforceSettings>
{
    public void Configure(EntityTypeBuilder<WorkforceSettings> builder)
    {
        builder.ToTable("WorkforceSettings");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.TenantId).IsUnique();

        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.ShiftToleranceMinutes).IsRequired();
    }
}
