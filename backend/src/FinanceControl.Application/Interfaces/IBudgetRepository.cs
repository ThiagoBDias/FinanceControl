using FinanceControl.Domain.Entities;

namespace FinanceControl.Application.Interfaces;

public interface IBudgetRepository
{
    Task<Budget?> GetByIdAsync(Guid budgetId, Guid ownerScopeId, CancellationToken cancellationToken = default);
    Task<Budget?> GetByCategoryMonthAsync(Guid ownerScopeId, Guid categoryId, int year, int month, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Budget>> ListByMonthAsync(Guid ownerScopeId, int year, int month, CancellationToken cancellationToken = default);
    Task AddAsync(Budget budget, CancellationToken cancellationToken = default);
}
