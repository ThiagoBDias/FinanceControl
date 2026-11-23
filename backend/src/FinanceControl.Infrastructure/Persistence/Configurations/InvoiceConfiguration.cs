using FinanceControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceControl.Infrastructure.Persistence.Configurations;

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedNever();

        builder.Property(i => i.OwnerScopeId).IsRequired();
        builder.Property(i => i.CreditCardAccountId).IsRequired();
        builder.Property(i => i.PeriodYear).IsRequired();
        builder.Property(i => i.PeriodMonth).IsRequired();

        builder.Property(i => i.ClosingDate).HasColumnType("date").IsRequired();
        builder.Property(i => i.DueDate).HasColumnType("date").IsRequired();

        builder.Property(i => i.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.Property(i => i.OpeningAmount).HasPrecision(18, 2).IsRequired();
        builder.Property(i => i.PaidAmount).HasPrecision(18, 2).IsRequired();

        builder.Property(i => i.CreatedAt).HasColumnType("timestamptz").IsRequired();
        builder.Property(i => i.UpdatedAt).HasColumnType("timestamptz").IsRequired();

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(i => i.CreditCardAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.OwnerScopeId);

        // Nao pedido explicitamente, mas protege exatamente o invariante
        // que RegisterExpenseUseCase assume (no maximo 1 fatura por
        // conta+periodo) contra uma corrida (2 requests simultaneos
        // tentando abrir a fatura do mesmo mes ao mesmo tempo).
        builder.HasIndex(i => new { i.CreditCardAccountId, i.PeriodYear, i.PeriodMonth }).IsUnique();

        builder.Property<uint>("xmin").IsRowVersion();
    }
}
