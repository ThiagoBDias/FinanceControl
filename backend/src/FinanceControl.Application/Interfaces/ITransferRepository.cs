using FinanceControl.Domain.Entities;

namespace FinanceControl.Application.Interfaces;

public interface ITransferRepository
{
    Task<TransferGroup?> GetByIdAsync(Guid transferGroupId, Guid ownerScopeId, CancellationToken cancellationToken = default);

    Task AddAsync(TransferGroup transferGroup, CancellationToken cancellationToken = default);
}
