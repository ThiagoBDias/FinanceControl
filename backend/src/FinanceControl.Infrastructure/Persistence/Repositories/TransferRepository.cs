using FinanceControl.Application.Interfaces;
using FinanceControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceControl.Infrastructure.Persistence.Repositories;

public sealed class TransferRepository : ITransferRepository
{
    private readonly FinanceControlDbContext _context;

    public TransferRepository(FinanceControlDbContext context) => _context = context;

    public async Task<TransferGroup?> GetByIdAsync(Guid transferGroupId, Guid ownerScopeId, CancellationToken cancellationToken = default)
    {
        return await _context.TransferGroups
            .SingleOrDefaultAsync(g => g.Id == transferGroupId && g.OwnerScopeId == ownerScopeId, cancellationToken);
    }

    public async Task AddAsync(TransferGroup transferGroup, CancellationToken cancellationToken = default)
    {
        await _context.TransferGroups.AddAsync(transferGroup, cancellationToken);
    }
}
