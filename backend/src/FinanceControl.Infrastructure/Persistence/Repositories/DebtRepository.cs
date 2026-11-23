using FinanceControl.Application.Interfaces;
using FinanceControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceControl.Infrastructure.Persistence.Repositories;

public sealed class DebtRepository(FinanceControlDbContext db) : IDebtRepository
{
    public async Task<IReadOnlyList<Debt>> ListAsync(Guid owner, CancellationToken cancellationToken) =>
        await db.Debts.AsNoTracking().Where(d => d.OwnerScopeId == owner).Include(d => d.Installments)
            .OrderBy(d => d.Name).ToListAsync(cancellationToken);

    public Task<Debt?> GetAsync(Guid owner, Guid debtId, CancellationToken cancellationToken) =>
        db.Debts.Include(d => d.Installments).SingleOrDefaultAsync(
            d => d.Id == debtId && d.OwnerScopeId == owner, cancellationToken);

    public Task<bool> IsPaymentAsync(Guid owner, Guid transactionId, CancellationToken cancellationToken) =>
        db.Debts.Where(d => d.OwnerScopeId == owner)
            .AnyAsync(d => d.Installments.Any(i => i.TransactionId == transactionId), cancellationToken);

    public void Add(Debt debt) => db.Debts.Add(debt);
}
