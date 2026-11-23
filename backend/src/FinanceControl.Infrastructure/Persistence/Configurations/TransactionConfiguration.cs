using FinanceControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceControl.Infrastructure.Persistence.Configurations;

public sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.OwnerScopeId).IsRequired();
        builder.Property(t => t.AccountId).IsRequired();

        builder.Property(t => t.Type).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(t => t.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(t => t.IsScheduled);
        builder.Property(t => t.PurchaseId);
        builder.Property(t => t.InstallmentNumber);
        builder.Property(t => t.InstallmentCount);
        builder.Property(t => t.Description).IsRequired().HasMaxLength(500);

        // Datas de negocio (sem horario/fuso) - regra "datas de
        // competencia e vencimento" da arquitetura original.
        builder.Property(t => t.CompetenceDate).HasColumnType("date").IsRequired();
        builder.Property(t => t.DueDate).HasColumnType("date");
        builder.Property(t => t.PaymentDate).HasColumnType("date");

        builder.Property(t => t.IsDeleted).IsRequired();
        builder.Property(t => t.DeletedAt).HasColumnType("timestamptz");

        // Reservados para Open Finance - sem uso ainda, so estrutura.
        builder.Property(t => t.Provider).HasMaxLength(100);
        builder.Property(t => t.ExternalAccountId).HasMaxLength(200);
        builder.Property(t => t.ExternalTransactionId).HasMaxLength(200);
        builder.Property(t => t.DedupHash).HasMaxLength(128);

        builder.Property(t => t.CreatedAt).HasColumnType("timestamptz").IsRequired();
        builder.Property(t => t.UpdatedAt).HasColumnType("timestamptz").IsRequired();

        // FKs - todas Restrict (regra obrigatoria: sem cascade perigoso).
        // A FK para Account ja e configurada pelo lado de Account
        // (HasMany), entao nao repetimos aqui para nao duplicar.

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Invoice>()
            .WithMany()
            .HasForeignKey(t => t.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<TransferGroup>()
            .WithMany()
            .HasForeignKey(t => t.TransferGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indices obrigatorios.
        builder.HasIndex(t => t.OwnerScopeId);
        builder.HasIndex(t => new { t.AccountId, t.CompetenceDate });
        builder.HasIndex(t => t.InvoiceId);
        builder.HasIndex(t => t.TransferGroupId);

        // DedupHash: indice NAO-unico (regra explicita: nunca bloqueia
        // automaticamente, so acelera a busca por possiveis duplicatas).
        builder.HasIndex(t => t.DedupHash);

        // Chave forte de deduplicacao futura (Provider+ExternalAccountId+
        // ExternalTransactionId): indice UNICO, mas filtrado - so vale
        // quando ExternalTransactionId nao e nulo. Hoje (tudo manual,
        // tudo null) nao bloqueia nada; fica pronta para quando o Open
        // Finance popular esses campos.
        builder.HasIndex(t => new { t.Provider, t.ExternalAccountId, t.ExternalTransactionId })
            .IsUnique()
            .HasFilter("\"ExternalTransactionId\" IS NOT NULL");

        // Soft delete: filtro global, nenhum repository precisa repetir
        // "Where(t => !t.IsDeleted)" manualmente.
        builder.HasQueryFilter(t => !t.IsDeleted);

        builder.Property<uint>("xmin").IsRowVersion();
    }
}
