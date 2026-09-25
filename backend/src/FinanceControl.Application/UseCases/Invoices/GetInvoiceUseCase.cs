using FinanceControl.Application.DTOs;
using FinanceControl.Application.Interfaces;
using FinanceControl.Application.Validators;
using FinanceControl.Domain.Entities;

namespace FinanceControl.Application.UseCases.Invoices;

public sealed class GetInvoiceUseCase : IUseCase<GetInvoiceRequest, InvoiceResponse>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ITransactionRepository _transactionRepository;

    public GetInvoiceUseCase(IInvoiceRepository invoiceRepository, ITransactionRepository transactionRepository)
    {
        _invoiceRepository = invoiceRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<InvoiceResponse> ExecuteAsync(GetInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId, request.OwnerScopeId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Invoice), request.InvoiceId);

        var totalAmount = await _transactionRepository.GetInvoiceTotalAsync(invoice.Id, cancellationToken);

        return InvoiceMapper.ToResponse(invoice, totalAmount);
    }
}
