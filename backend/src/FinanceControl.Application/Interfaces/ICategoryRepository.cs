using FinanceControl.Domain.Entities;

namespace FinanceControl.Application.Interfaces;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid categoryId, Guid ownerScopeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Category>> ListByOwnerAsync(Guid ownerScopeId, CancellationToken cancellationToken = default);

    Task AddAsync(Category category, CancellationToken cancellationToken = default);
}
