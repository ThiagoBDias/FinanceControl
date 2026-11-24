using FinanceControl.Application.Interfaces;
using FinanceControl.Domain.Entities;
using FinanceControl.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinanceControl.Infrastructure.Persistence.Repositories;

public sealed class InvoiceRepository : IInvoiceRepository
{
    private readonly FinanceControlDbContext _context;

    public InvoiceRepository(FinanceControlDbContext context) => _context = context;

    public async Task<Invoice?> GetByIdAsync(Guid invoiceId, Guid ownerScopeId, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .SingleOrDefaultAsync(i => i.Id == invoiceId && i.OwnerScopeId == ownerScopeId, cancellationToken);
    }

    public async Task<Invoice?> GetPeriodInvoiceAsync(Guid creditCardAccountId, int periodYear, int periodMonth, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .SingleOrDefaultAsync(
                i => i.CreditCardAccountId == creditCardAccountId
                     && i.PeriodYear == periodYear
                     && i.PeriodMonth == periodMonth,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Invoice>> ListByCreditCardAccountAsync(Guid creditCardAccountId, Guid ownerScopeId, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Where(i => i.CreditCardAccountId == creditCardAccountId && i.OwnerScopeId == ownerScopeId)
            .OrderByDescending(i => i.PeriodYear)
            .ThenByDescending(i => i.PeriodMonth)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        await _context.Invoices.AddAsync(invoice, cancellationToken);
    }
}
