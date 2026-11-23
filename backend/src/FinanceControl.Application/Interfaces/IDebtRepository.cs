using FinanceControl.Domain.Entities;

namespace FinanceControl.Application.Interfaces;

public interface IDebtRepository
{
    Task<IReadOnlyList<Debt>> ListAsync(Guid owner, CancellationToken cancellationToken);
    Task<Debt?> GetAsync(Guid owner, Guid debtId, CancellationToken cancellationToken);
    Task<bool> IsPaymentAsync(Guid owner, Guid transactionId, CancellationToken cancellationToken);
    void Add(Debt debt);
}
