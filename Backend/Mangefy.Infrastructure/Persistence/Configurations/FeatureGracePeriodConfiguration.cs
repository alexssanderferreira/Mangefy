using Mangefy.Domain.Platform.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mangefy.Infrastructure.Persistence.Configurations;

public sealed class FeatureGracePeriodConfiguration : IEntityTypeConfiguration<FeatureGracePeriod>
{
    public void Configure(EntityTypeBuilder<FeatureGracePeriod> builder)
    {
        builder.ToTable("FeatureGracePeriods");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.TenantId, x.FeatureKey });

        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.FeatureKey).IsRequired().HasMaxLength(100);
        builder.Property(x => x.ExpiresAt).IsRequired();
        builder.Property(x => x.NotifiedAt);

        builder.Ignore(x => x.IsExpired);
    }
}
