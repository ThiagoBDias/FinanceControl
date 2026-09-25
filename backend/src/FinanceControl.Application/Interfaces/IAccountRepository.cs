using FinanceControl.Domain.Entities;

namespace FinanceControl.Application.Interfaces;

/// <summary>
/// GetByIdAsync deve retornar a conta com suas Transactions carregadas
/// (necessario para CalculatedBalance). Nao ha metodo Update: mutacoes
/// feitas no Account retornado por GetByIdAsync sao persistidas via
/// IUnitOfWork.SaveChangesAsync (entidade ja rastreada pela Infrastructure).
/// </summary>
public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid accountId, Guid ownerScopeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Account>> ListByOwnerAsync(Guid ownerScopeId, CancellationToken cancellationToken = default);

    Task AddAsync(Account account, CancellationToken cancellationToken = default);
}
