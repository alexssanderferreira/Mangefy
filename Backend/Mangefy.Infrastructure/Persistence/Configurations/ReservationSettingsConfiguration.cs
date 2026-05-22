using Mangefy.Domain.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mangefy.Infrastructure.Persistence.Configurations;

public sealed class ReservationSettingsConfiguration : IEntityTypeConfiguration<ReservationSettings>
{
    public void Configure(EntityTypeBuilder<ReservationSettings> builder)
    {
        builder.ToTable("ReservationSettings");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.TenantId).IsUnique();

        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.MaxSimultaneousReservations);
    }
}
