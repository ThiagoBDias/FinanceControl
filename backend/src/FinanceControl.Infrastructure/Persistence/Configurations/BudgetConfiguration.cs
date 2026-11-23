using FinanceControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceControl.Infrastructure.Persistence.Configurations;

public sealed class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).ValueGeneratedNever();
        builder.Property(b => b.OwnerScopeId).IsRequired();
        builder.Property(b => b.CategoryId).IsRequired();
        builder.Property(b => b.Year).IsRequired();
        builder.Property(b => b.Month).IsRequired();
        builder.Property(b => b.LimitAmount).HasPrecision(18, 2).IsRequired();
        builder.Property(b => b.CreatedAt).HasColumnType("timestamptz").IsRequired();
        builder.Property(b => b.UpdatedAt).HasColumnType("timestamptz").IsRequired();
        builder.HasIndex(b => new { b.OwnerScopeId, b.CategoryId, b.Year, b.Month }).IsUnique();
        builder.HasIndex(b => new { b.OwnerScopeId, b.Year, b.Month });
        builder.Property<uint>("xmin").IsRowVersion();
    }
}
