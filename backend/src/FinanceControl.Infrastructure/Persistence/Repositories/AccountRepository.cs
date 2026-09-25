using FinanceControl.Application.Interfaces;
using FinanceControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceControl.Infrastructure.Persistence.Repositories;

public sealed class AccountRepository : IAccountRepository
{
    private readonly FinanceControlDbContext _context;

    public AccountRepository(FinanceControlDbContext context) => _context = context;

    public async Task<Account?> GetByIdAsync(Guid accountId, Guid ownerScopeId, CancellationToken cancellationToken = default)
    {
        // Include(Transactions) e obrigatorio: Account.CalculatedBalance
        // (usado por AccountMapper.ToResponse) e recalculado em memoria a
        // partir da colecao - sem o Include, o saldo viria sempre igual a
        // InitialBalance.
        return await _context.Accounts
            .Include(a => a.Transactions)
            .SingleOrDefaultAsync(a => a.Id == accountId && a.OwnerScopeId == ownerScopeId, cancellationToken);
    }

    public async Task<IReadOnlyList<Account>> ListByOwnerAsync(Guid ownerScopeId, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .Include(a => a.Transactions)
            .Where(a => a.OwnerScopeId == ownerScopeId)
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Account account, CancellationToken cancellationToken = default)
    {
        await _context.Accounts.AddAsync(account, cancellationToken);
    }
}
