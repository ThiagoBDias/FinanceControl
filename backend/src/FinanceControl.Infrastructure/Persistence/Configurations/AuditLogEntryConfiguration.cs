using FinanceControl.Infrastructure.Persistence.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceControl.Infrastructure.Persistence.Configurations;

internal sealed class AuditLogEntryConfiguration : IEntityTypeConfiguration<AuditLogEntry>
{
    public void Configure(EntityTypeBuilder<AuditLogEntry> builder)
    {
        builder.ToTable("AuditLogEntries");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.OwnerScopeId).IsRequired();
        builder.Property(a => a.EntityName).IsRequired().HasMaxLength(100);
        builder.Property(a => a.EntityId).IsRequired();
        builder.Property(a => a.Action).IsRequired().HasMaxLength(20);
        builder.Property(a => a.ChangesJson).IsRequired().HasColumnType("jsonb");
        builder.Property(a => a.CreatedAt).HasColumnType("timestamptz").IsRequired();

        builder.HasIndex(a => a.OwnerScopeId);
        builder.HasIndex(a => new { a.EntityName, a.EntityId });

        // Append-only de VERDADE (bloqueio no banco, nao so na aplicacao)
        // exige, na migration inicial (proxima etapa):
        //   REVOKE UPDATE, DELETE ON "AuditLogEntries" FROM <app_role>;
        // Ate la, o unico bloqueio e estrutural: nenhum repository ou
        // caso de uso tem um metodo que atualize/remova AuditLogEntry.
    }
}
