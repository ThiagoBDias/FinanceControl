using FinanceControl.Application.Interfaces;

namespace FinanceControl.Infrastructure.Persistence;

/// <summary>
/// Uma unica chamada a DbContext.SaveChangesAsync ja e atomica (roda
/// dentro de uma transacao implicita do EF Core / Postgres) - por isso
/// esta implementacao e so um repasse fino. E exatamente o que
/// CreateTransfer (2 Accounts + 1 TransferGroup, + a auditoria de tudo isso)
/// precisa: como
/// os repositories so fazem AddAsync/mutacao em memoria (sem chamar
/// SaveChanges), tudo fica pendente no mesmo ChangeTracker ate este
/// SaveChangesAsync ser chamado uma unica vez no fim do caso de uso -
/// todas as entidades tocadas commitam juntas, ou nenhuma commita.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly FinanceControlDbContext _context;

    public UnitOfWork(FinanceControlDbContext context) => _context = context;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
