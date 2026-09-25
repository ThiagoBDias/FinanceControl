using FinanceControl.Domain.Entities;
using FinanceControl.Infrastructure.Persistence.Audit;
using FinanceControl.Infrastructure.Auth;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FinanceControl.Infrastructure.Persistence;

public sealed class FinanceControlDbContext : IdentityDbContext<ApplicationUser, Microsoft.AspNetCore.Identity.IdentityRole<Guid>, Guid>
{
    public FinanceControlDbContext(DbContextOptions<FinanceControlDbContext> options) : base(options)
    {
    }

    public DbSet<User> DomainUsers => Set<User>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<TransferGroup> TransferGroups => Set<TransferGroup>();
    public DbSet<Budget> Budgets => Set<Budget>();

    /// <summary>
    /// Nao e exposta como propriedade publica de proposito - ninguem fora
    /// deste DbContext deve gravar AuditLogEntry diretamente (nem sequer
    /// os repositories tem essa opcao). Ver SaveChangesAsync.
    /// </summary>
    internal DbSet<AuditLogEntry> AuditLogEntries => Set<AuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinanceControlDbContext).Assembly);
    }

    /// <summary>
    /// Captura o diff do ChangeTracker ANTES de salvar e grava as
    /// AuditLogEntry correspondentes na MESMA chamada de SaveChanges -
    /// mudanca de negocio e auditoria sao atomicas (ou as duas persistem,
    /// ou nenhuma). Ver AuditLogEntryFactory para o que e auditado.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditEntries = AuditLogEntryFactory.Build(ChangeTracker);
        if (auditEntries.Count > 0)
            AuditLogEntries.AddRange(auditEntries);

        return await base.SaveChangesAsync(cancellationToken);
    }
}
