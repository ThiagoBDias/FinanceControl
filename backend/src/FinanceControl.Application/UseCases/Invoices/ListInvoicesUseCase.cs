using FinanceControl.Application.DTOs;
using FinanceControl.Application.Interfaces;

namespace FinanceControl.Application.UseCases.Invoices;

public sealed class ListInvoicesUseCase : IUseCase<ListInvoicesRequest, IReadOnlyList<InvoiceResponse>>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ITransactionRepository _transactionRepository;

    public ListInvoicesUseCase(IInvoiceRepository invoiceRepository, ITransactionRepository transactionRepository)
    {
        _invoiceRepository = invoiceRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<IReadOnlyList<InvoiceResponse>> ExecuteAsync(ListInvoicesRequest request, CancellationToken cancellationToken = default)
    {
        var invoices = await _invoiceRepository.ListByCreditCardAccountAsync(
            request.CreditCardAccountId, request.OwnerScopeId, cancellationToken);

        var responses = new List<InvoiceResponse>(invoices.Count);
        foreach (var invoice in invoices)
        {
            // N+1 aceitavel no MVP (poucas faturas por conta); pode virar
            // uma unica query com agregacao quando a Infrastructure existir.
            var totalAmount = invoice.OpeningAmount + await _transactionRepository.GetInvoiceTotalAsync(invoice.Id, cancellationToken);
            responses.Add(InvoiceMapper.ToResponse(invoice, totalAmount));
        }

        return responses;
    }
}
