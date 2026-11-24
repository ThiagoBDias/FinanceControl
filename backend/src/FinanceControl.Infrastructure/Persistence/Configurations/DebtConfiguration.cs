using FinanceControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceControl.Infrastructure.Persistence.Configurations;

public sealed class DebtConfiguration : IEntityTypeConfiguration<Debt>
{
    public void Configure(EntityTypeBuilder<Debt> builder)
    {
        builder.Property(d => d.Id).ValueGeneratedNever();
        builder.Property(d => d.Name).HasMaxLength(100);
        builder.Property(d => d.TotalAmount).HasPrecision(18, 2);
        builder.HasIndex(d => d.OwnerScopeId);
        builder.HasMany(d => d.Installments).WithOne().HasForeignKey(i => i.DebtId).OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(d => d.Installments).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasOne<Category>().WithMany().HasForeignKey(d => d.CategoryId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class DebtInstallmentConfiguration : IEntityTypeConfiguration<DebtInstallment>
{
    public void Configure(EntityTypeBuilder<DebtInstallment> builder)
    {
        builder.HasKey(i => new { i.DebtId, i.Number });
        builder.Property(i => i.Amount).HasPrecision(18, 2);
        builder.Property(i => i.DueDate);
        builder.HasOne<Transaction>().WithOne().HasForeignKey<DebtInstallment>(i => i.TransactionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
