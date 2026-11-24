using FinanceControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceControl.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedNever();

        builder.Property(u => u.Name).IsRequired().HasMaxLength(200);

        // Email e Value Object (ValueObjects/Email.cs) - owned type, mesma
        // tabela (Value armazenado como coluna "Email" em Users).
        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .IsRequired()
                .HasMaxLength(320);

            email.HasIndex(e => e.Value).IsUnique();
        });

        builder.Property(u => u.CreatedAt).HasColumnType("timestamptz").IsRequired();
        builder.Property(u => u.UpdatedAt).HasColumnType("timestamptz").IsRequired();

        // Concorrencia otimista via coluna de sistema xmin do Postgres -
        // nenhuma coluna extra e criada (ver estrategia definida na
        // revisao arquitetural, rodada 2).
        builder.Property<uint>("xmin").IsRowVersion();
    }
}
