using FinanceControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceControl.Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.OwnerScopeId).IsRequired();
        builder.Property(c => c.Name).IsRequired().HasMaxLength(150);
        builder.Property(c => c.Type).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(c => c.IsActive).IsRequired();

        builder.Property(c => c.CreatedAt).HasColumnType("timestamptz").IsRequired();
        builder.Property(c => c.UpdatedAt).HasColumnType("timestamptz").IsRequired();

        builder.HasIndex(c => c.OwnerScopeId);

        // Nao pedido explicitamente - adicionei porque duas categorias
        // com o mesmo nome para o mesmo usuario e um erro de dados quase
        // certo, nao um caso de uso legitimo.
        builder.HasIndex(c => new { c.OwnerScopeId, c.Name }).IsUnique();

        builder.Property<uint>("xmin").IsRowVersion();
    }
}
