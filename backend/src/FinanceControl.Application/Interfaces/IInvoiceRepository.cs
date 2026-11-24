using FinanceControl.Domain.Entities;

namespace FinanceControl.Application.Interfaces;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(Guid invoiceId, Guid ownerScopeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca a fatura em aberto (Status = Open) de uma conta de cartao para
    /// o periodo informado. Usado por RegisterExpenseUseCase para
    /// encontrar-ou-abrir a fatura de uma compra nova.
    /// </summary>
    Task<Invoice?> GetPeriodInvoiceAsync(Guid creditCardAccountId, int periodYear, int periodMonth, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Invoice>> ListByCreditCardAccountAsync(Guid creditCardAccountId, Guid ownerScopeId, CancellationToken cancellationToken = default);

    Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default);
}
