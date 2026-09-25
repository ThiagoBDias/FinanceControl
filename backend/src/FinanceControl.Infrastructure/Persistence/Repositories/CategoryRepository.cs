using FinanceControl.Application.Interfaces;
using FinanceControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceControl.Infrastructure.Persistence.Repositories;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly FinanceControlDbContext _context;

    public CategoryRepository(FinanceControlDbContext context) => _context = context;

    public async Task<Category?> GetByIdAsync(Guid categoryId, Guid ownerScopeId, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .SingleOrDefaultAsync(c => c.Id == categoryId && c.OwnerScopeId == ownerScopeId, cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> ListByOwnerAsync(Guid ownerScopeId, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .Where(c => c.OwnerScopeId == ownerScopeId)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Category category, CancellationToken cancellationToken = default)
    {
        await _context.Categories.AddAsync(category, cancellationToken);
    }
}
