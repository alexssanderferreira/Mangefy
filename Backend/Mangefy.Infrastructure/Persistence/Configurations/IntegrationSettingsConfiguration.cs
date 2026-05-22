using Mangefy.Domain.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mangefy.Infrastructure.Persistence.Configurations;

public sealed class IntegrationSettingsConfiguration : IEntityTypeConfiguration<IntegrationSettings>
{
    public void Configure(EntityTypeBuilder<IntegrationSettings> builder)
    {
        builder.ToTable("IntegrationSettings");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.TenantId).IsUnique();

        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.DeliveryIntegrationEnabled).IsRequired();
    }
}
