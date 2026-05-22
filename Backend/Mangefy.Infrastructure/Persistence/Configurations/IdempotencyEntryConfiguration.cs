using Mangefy.Domain.Idempotency;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mangefy.Infrastructure.Persistence.Configurations;

public sealed class IdempotencyEntryConfiguration : IEntityTypeConfiguration<IdempotencyEntry>
{
    public void Configure(EntityTypeBuilder<IdempotencyEntry> builder)
    {
        builder.ToTable("IdempotencyEntries");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.TenantId, x.CommandId }).IsUnique();

        builder.Property(x => x.CommandName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.ResponseJson).HasMaxLength(4000);
        builder.Property(x => x.ExpiresAt).IsRequired();
    }
}
