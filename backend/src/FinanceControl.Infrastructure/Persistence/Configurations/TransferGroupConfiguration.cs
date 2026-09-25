using FinanceControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceControl.Infrastructure.Persistence.Configurations;

public sealed class TransferGroupConfiguration : IEntityTypeConfiguration<TransferGroup>
{
    public void Configure(EntityTypeBuilder<TransferGroup> builder)
    {
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id).ValueGeneratedNever();

        builder.Property(g => g.OwnerScopeId).IsRequired();
        builder.Property(g => g.SourceAccountId).IsRequired();
        builder.Property(g => g.DestinationAccountId).IsRequired();

        builder.Property(g => g.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(g => g.TransferDate).HasColumnType("date").IsRequired();
        builder.Property(g => g.Description).HasMaxLength(500);

        builder.Property(g => g.CreatedAt).HasColumnType("timestamptz").IsRequired();

        // Duas FKs distintas para Account (origem/destino) - precisam ser
        // configuradas explicitamente, o EF nao infere automaticamente
        // qual e qual quando ha mais de uma FK para a mesma tabela.
        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(g => g.SourceAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(g => g.DestinationAccountId)
            .OnDelete(DeleteBehavior.Restrict);


        builder.HasIndex(g => g.OwnerScopeId);
        builder.HasIndex(g => g.SourceAccountId);
        builder.HasIndex(g => g.DestinationAccountId);

        // SEM UseXminAsConcurrencyToken() aqui: TransferGroup nao tem
        // UpdatedAt e o Domain nao expoe nenhum metodo de mutacao nele -
        // e criado uma vez e nunca alterado, entao nao ha concorrencia de
        // escrita para proteger.
    }
}
