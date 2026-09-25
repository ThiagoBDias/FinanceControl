using FinanceControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceControl.Infrastructure.Persistence.Configurations;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.OwnerScopeId).IsRequired();
        builder.Property(a => a.Name).IsRequired().HasMaxLength(200);

        // Regra "enums devem ter conversao explicita".
        builder.Property(a => a.Type).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.Property(a => a.InitialBalance).HasPrecision(18, 2).IsRequired();

        // CachedBalance e SOMENTE cache de performance (mantido
        // incrementalmente pelo proprio Account.AttachTransaction) -
        // nunca e a fonte de verdade. CalculatedBalance (a fonte real) e
        // Ignore()d logo abaixo, porque e recalculada em memoria a partir
        // de InitialBalance + Transactions e NAO existe como coluna.
        builder.Property(a => a.CachedBalance).HasPrecision(18, 2).IsRequired();
        builder.Property(a => a.CachedBalanceUpdatedAt).HasColumnType("timestamptz").IsRequired();

        builder.Property(a => a.IsActive).IsRequired();
        builder.Property(a => a.CreatedAt).HasColumnType("timestamptz").IsRequired();
        builder.Property(a => a.UpdatedAt).HasColumnType("timestamptz").IsRequired();

        // Cartoes sao opcionais na Account. O owned type fica em tabela
        // separada para evitar a ambiguidade de optional owned + table splitting.
        builder.OwnsOne(a => a.CreditCardSettings, cc =>
        {
            cc.ToTable("CreditCardSettings");
            cc.Property(c => c.CreditLimit).HasPrecision(18, 2).IsRequired();
            cc.Property(c => c.ClosingDay).IsRequired();
            cc.Property(c => c.DueDay).IsRequired();
        });

        // CalculatedBalance e Nature/IsLiability nao sao colunas - sao
        // calculadas em memoria a partir do ledger. Sem este Ignore, o EF
        // tentaria mapear CalculatedBalance (get-only) como coluna.
        builder.Ignore(a => a.CalculatedBalance);

        // Account.Transactions e IReadOnlyCollection<Transaction> (sem
        // setter) apoiada no campo privado _transactions - por isso o
        // Navigation().UsePropertyAccessMode(Field) abaixo: o EF passa a
        // ler/escrever direto no campo, nunca pela propriedade publica.
        builder.HasMany(a => a.Transactions)
            .WithOne()
            .HasForeignKey(t => t.AccountId)
            // Regra obrigatoria: nenhum cascade delete em dado financeiro.
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(a => a.Transactions).UsePropertyAccessMode(PropertyAccessMode.Field);

        // Regra obrigatoria: OwnerScopeId indexado.
        builder.HasIndex(a => a.OwnerScopeId);

        builder.UseXminAsConcurrencyToken();
    }
}
