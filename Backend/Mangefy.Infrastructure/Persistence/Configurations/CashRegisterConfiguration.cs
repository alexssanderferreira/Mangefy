using Mangefy.Domain.DailyCash;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mangefy.Infrastructure.Persistence.Configurations;

public sealed class CashRegisterConfiguration : IEntityTypeConfiguration<CashRegister>
{
    public void Configure(EntityTypeBuilder<CashRegister> builder)
    {
        builder.ToTable("CashRegisters");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RowVersion).HasColumnName("xmin").HasColumnType("xid").IsRowVersion();

        builder.OwnsOne(x => x.OpeningAmount, m =>
        {
            m.Property(x => x.Amount).HasColumnName("OpeningAmount").HasColumnType("decimal(10,2)");
            m.Property(x => x.Currency).HasColumnName("OpeningCurrency").HasMaxLength(3);
        });

        builder.OwnsOne(x => x.ClosingAmount, m =>
        {
            m.Property(x => x.Amount).HasColumnName("ClosingAmount").HasColumnType("decimal(10,2)");
            m.Property(x => x.Currency).HasColumnName("ClosingCurrency").HasMaxLength(3);
        });

        builder.OwnsOne(x => x.ExpectedAmount, m =>
        {
            m.Property(x => x.Amount).HasColumnName("ExpectedAmount").HasColumnType("decimal(10,2)");
            m.Property(x => x.Currency).HasColumnName("ExpectedCurrency").HasMaxLength(3);
        });

        builder.Property(x => x.ClosingNotes).HasMaxLength(1000);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);

        builder.OwnsMany(x => x.Withdrawals, w =>
        {
            w.ToTable("CashWithdrawals");
            w.HasKey(x => x.Id);
            w.WithOwner().HasForeignKey("CashRegisterId");
            w.OwnsOne(x => x.Amount, m =>
            {
                m.Property(x => x.Amount).HasColumnName("Amount").HasColumnType("decimal(10,2)");
                m.Property(x => x.Currency).HasColumnName("Currency").HasMaxLength(3);
            });
            w.Property(x => x.Reason).IsRequired().HasMaxLength(500);
            w.Property(x => x.EmployeeId).IsRequired();
        });

        builder.OwnsMany(x => x.Supplies, s =>
        {
            s.ToTable("CashSupplies");
            s.HasKey(x => x.Id);
            s.WithOwner().HasForeignKey("CashRegisterId");
            s.OwnsOne(x => x.Amount, m =>
            {
                m.Property(x => x.Amount).HasColumnName("Amount").HasColumnType("decimal(10,2)");
                m.Property(x => x.Currency).HasColumnName("Currency").HasMaxLength(3);
            });
            s.Property(x => x.Reason).IsRequired().HasMaxLength(500);
            s.Property(x => x.EmployeeId).IsRequired();
        });

        builder.OwnsMany(x => x.MethodBalances, mb =>
        {
            mb.ToTable("CashMethodBalances");
            mb.Property<int>("Id").ValueGeneratedOnAdd();
            mb.HasKey("Id");
            mb.WithOwner().HasForeignKey("CashRegisterId");
            mb.Property(x => x.Method).HasConversion<string>().HasMaxLength(30);
            mb.Property(x => x.ExpectedAmount).HasColumnType("decimal(10,2)");
            mb.Property(x => x.CountedAmount).HasColumnType("decimal(10,2)");
        });
    }
}
