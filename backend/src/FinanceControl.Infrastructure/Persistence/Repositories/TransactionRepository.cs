using FinanceControl.Application.DTOs;
using FinanceControl.Application.Interfaces;
using FinanceControl.Domain.Entities;
using FinanceControl.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinanceControl.Infrastructure.Persistence.Repositories;

/// <summary>
/// Somente leitura (ver ITransactionRepository) - a escrita acontece
/// atraves do aggregate Account, nao aqui.
/// </summary>
public sealed class TransactionRepository : ITransactionRepository
{
    private readonly FinanceControlDbContext _context;

    public TransactionRepository(FinanceControlDbContext context) => _context = context;

    public async Task<Transaction?> GetByIdAsync(Guid transactionId, Guid ownerScopeId, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .SingleOrDefaultAsync(t => t.Id == transactionId && t.OwnerScopeId == ownerScopeId, cancellationToken);
    }

    public async Task<IReadOnlyList<Transaction>> ListAsync(ListTransactionsRequest filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Transactions
            .Where(t => t.OwnerScopeId == filter.OwnerScopeId);

        if (filter.AccountId is Guid accountId)
            query = query.Where(t => t.AccountId == accountId);

        if (filter.CategoryId is Guid categoryId)
            query = query.Where(t => t.CategoryId == categoryId);

        if (filter.InvoiceId is Guid invoiceId)
            query = query.Where(t => t.InvoiceId == invoiceId);

        if (filter.Type is TransactionType type)
            query = query.Where(t => t.Type == type);

        if (filter.CompetenceDateFrom is DateOnly from)
            query = query.Where(t => t.CompetenceDate >= from);

        if (filter.CompetenceDateTo is DateOnly to)
            query = query.Where(t => t.CompetenceDate <= to);

        return await query
            .OrderByDescending(t => t.CompetenceDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetExpenseTotalByCategoryAndMonthAsync(Guid ownerScopeId, Guid categoryId, int year, int month, CancellationToken cancellationToken = default)
    {
        var from = new DateOnly(year, month, 1);
        var to = from.AddMonths(1).AddDays(-1);
        return await _context.Transactions
            .Where(t => t.OwnerScopeId == ownerScopeId
                     && t.CategoryId == categoryId
                     && t.Type == TransactionType.Expense
                     && t.CompetenceDate >= from
                     && t.CompetenceDate <= to)
            .SumAsync(t => (decimal?)t.Amount, cancellationToken) ?? 0m;
    }

    public async Task<decimal> GetInvoiceTotalAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        // Fonte da verdade do total da fatura (decisao da etapa Domain):
        // soma das Expense vinculadas, nunca uma coluna persistida.
        // O filtro global de soft-delete (HasQueryFilter em
        // TransactionConfiguration) ja exclui IsDeleted automaticamente.
        return await _context.Transactions
            .Where(t => t.InvoiceId == invoiceId && t.Type == TransactionType.Expense)
            .SumAsync(t => (decimal?)t.Amount, cancellationToken) ?? 0m;
    }
}
