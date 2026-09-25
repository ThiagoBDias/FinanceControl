using FinanceControl.Application.Interfaces;
using FinanceControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceControl.Infrastructure.Persistence.Repositories;

public sealed class BudgetRepository : IBudgetRepository
{
    private readonly FinanceControlDbContext _context;
    public BudgetRepository(FinanceControlDbContext context) => _context = context;

    public Task<Budget?> GetByIdAsync(Guid budgetId, Guid ownerScopeId, CancellationToken cancellationToken = default) =>
        _context.Budgets.SingleOrDefaultAsync(b => b.Id == budgetId && b.OwnerScopeId == ownerScopeId, cancellationToken);

    public Task<Budget?> GetByCategoryMonthAsync(Guid ownerScopeId, Guid categoryId, int year, int month, CancellationToken cancellationToken = default) =>
        _context.Budgets.SingleOrDefaultAsync(b => b.OwnerScopeId == ownerScopeId && b.CategoryId == categoryId && b.Year == year && b.Month == month, cancellationToken);

    public async Task<IReadOnlyList<Budget>> ListByMonthAsync(Guid ownerScopeId, int year, int month, CancellationToken cancellationToken = default) =>
        await _context.Budgets.Where(b => b.OwnerScopeId == ownerScopeId && b.Year == year && b.Month == month).OrderBy(b => b.CategoryId).ToListAsync(cancellationToken);

    public Task AddAsync(Budget budget, CancellationToken cancellationToken = default) => _context.Budgets.AddAsync(budget, cancellationToken).AsTask();
}
