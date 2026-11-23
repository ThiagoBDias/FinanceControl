using FinanceControl.Application.DTOs;
using FinanceControl.Domain.Entities;

namespace FinanceControl.Application.Interfaces;

/// <summary>
/// Somente leitura. A escrita de Transaction acontece atraves do aggregate
/// Account (RegisterIncome/RegisterExpense/RegisterTransferOut/In) - nao
/// existe AddAsync aqui de proposito, para nao permitir criar uma
/// Transaction desacoplada da conta que a validou.
/// </summary>
public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(Guid transactionId, Guid ownerScopeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Transaction>> ListAsync(ListTransactionsRequest filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soma das Transactions do tipo Expense vinculadas a fatura (InvoiceId).
    /// E como InvoiceResponse.TotalAmount e calculado, ja que Invoice nao
    /// guarda esse valor no Domain (decisao da rodada de ledger).
    /// </summary>
    Task<decimal> GetInvoiceTotalAsync(Guid invoiceId, CancellationToken cancellationToken = default);

    Task<decimal> GetExpenseTotalByCategoryAndMonthAsync(Guid ownerScopeId, Guid categoryId, int year, int month, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Transaction>> ListPendingExpensesAsync(Guid owner, CancellationToken cancellationToken);
}
