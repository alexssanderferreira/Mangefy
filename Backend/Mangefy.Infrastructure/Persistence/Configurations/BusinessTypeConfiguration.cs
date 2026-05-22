using Mangefy.Domain.Platform.BusinessTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mangefy.Infrastructure.Persistence.Configurations;

public sealed class BusinessTypeConfiguration : IEntityTypeConfiguration<BusinessType>
{
    public void Configure(EntityTypeBuilder<BusinessType> builder)
    {
        builder.ToTable("BusinessTypes");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(x => x.Name).IsUnique();
        builder.Property(x => x.Description).HasMaxLength(500);

        builder.OwnsMany(x => x.RoleTemplates, t =>
        {
            t.ToTable("RoleTemplates");
            t.HasKey(x => x.Id);
            t.Property(x => x.Name).IsRequired().HasMaxLength(100);
            t.Property(x => x.Description).HasMaxLength(500);

            t.Property<List<string>>("_permissions")
                .HasField("_permissions")
                .HasColumnName("Permissions")
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Length == 0 ? new List<string>() : v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
                .HasMaxLength(4000)
                .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));
        });

        builder.Navigation(x => x.RoleTemplates).AutoInclude();
    }
}
