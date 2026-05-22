using Mangefy.Domain.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mangefy.Infrastructure.Persistence.Configurations;

public sealed class FiscalSettingsConfiguration : IEntityTypeConfiguration<FiscalSettings>
{
    public void Configure(EntityTypeBuilder<FiscalSettings> builder)
    {
        builder.ToTable("FiscalSettings");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.TenantId).IsUnique();

        builder.Property(x => x.Cnpj).HasMaxLength(14);
        builder.Property(x => x.FiscalHubApiKey).HasMaxLength(500);
    }
}
