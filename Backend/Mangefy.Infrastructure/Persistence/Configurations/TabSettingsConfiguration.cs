using Mangefy.Domain.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mangefy.Infrastructure.Persistence.Configurations;

public sealed class TabSettingsConfiguration : IEntityTypeConfiguration<TabSettings>
{
    public void Configure(EntityTypeBuilder<TabSettings> builder)
    {
        builder.ToTable("TabSettings");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.TenantId).IsUnique();

        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.MinTabNumber).IsRequired();
        builder.Property(x => x.MaxTabNumber).IsRequired();
        builder.Property(x => x.MaxDiscountPercent).HasColumnType("decimal(5,2)").HasDefaultValue(10m);
        builder.Property(x => x.DiscountReasonRequiredAbove).HasColumnType("decimal(10,2)");

        builder.Ignore(x => x.TotalNumbers);
    }
}
