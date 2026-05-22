using Mangefy.Domain.Platform.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mangefy.Infrastructure.Persistence.Configurations;

public sealed class PlanFeatureSetConfiguration : IEntityTypeConfiguration<PlanFeatureSet>
{
    public void Configure(EntityTypeBuilder<PlanFeatureSet> builder)
    {
        builder.ToTable("PlanFeatureSets");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.PlanId, x.BusinessTypeId }).IsUnique();

        builder.Property<List<string>>("_enabledFeatures")
            .HasField("_enabledFeatures")
            .HasColumnName("Features")
            .HasConversion(
                v => string.Join(',', v),
                v => v.Length == 0 ? new List<string>() : v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
            .HasMaxLength(2000)
            .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                (c1, c2) => c1!.SequenceEqual(c2!),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()));
    }
}
