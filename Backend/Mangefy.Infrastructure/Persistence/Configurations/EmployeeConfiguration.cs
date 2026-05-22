using Mangefy.Domain.Employees;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mangefy.Infrastructure.Persistence.Configurations;

public sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RowVersion)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .IsRowVersion();

        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.OwnsOne(x => x.Email, e =>
        {
            e.Property(x => x.Value).HasColumnName("Email").IsRequired().HasMaxLength(200);
        });

        builder.Property(x => x.PasswordHash).IsRequired(false).HasMaxLength(500);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.TenantRoleId).IsRequired();
    }
}
