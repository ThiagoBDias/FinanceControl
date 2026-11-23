using FinanceControl.Application.DTOs;
using FinanceControl.Application.Interfaces;
using FinanceControl.Application.Validators;
using FinanceControl.Domain.Entities;

namespace FinanceControl.Application.UseCases.Invoices;

public sealed class CloseInvoiceUseCase : IUseCase<CloseInvoiceRequest, InvoiceResponse>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CloseInvoiceUseCase(
        IInvoiceRepository invoiceRepository,
        ITransactionRepository transactionRepository,
        IUnitOfWork unitOfWork)
    {
        _invoiceRepository = invoiceRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<InvoiceResponse> ExecuteAsync(CloseInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        if (request.OwnerScopeId == Guid.Empty || request.InvoiceId == Guid.Empty)
            throw new RequestValidationException(new[]
            {
                new ValidationError(nameof(request.InvoiceId), "OwnerScopeId e InvoiceId sao obrigatorios.")
            });

        var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId, request.OwnerScopeId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Invoice), request.InvoiceId);

        if (invoice.Status == FinanceControl.Domain.Enums.InvoiceStatus.Open)
            invoice.Close();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var totalAmount = invoice.OpeningAmount + await _transactionRepository.GetInvoiceTotalAsync(invoice.Id, cancellationToken);
        return InvoiceMapper.ToResponse(invoice, totalAmount);
    }
}
