using FinanceControl.Domain.Exceptions;
using FinanceControl.Application.DTOs;
using FinanceControl.Application.Interfaces;
using FinanceControl.Application.Validators;
using FinanceControl.Domain.Entities;
using FinanceControl.Domain.Enums;

namespace FinanceControl.Application.UseCases.Invoices;

public sealed class PayInvoiceUseCase(
    IInvoiceRepository invoices, IAccountRepository accounts, ITransactionRepository transactions,
    ITransferRepository transfers, IUnitOfWork unitOfWork)
    : IUseCase<PayInvoiceRequest, InvoiceResponse>
{
    public async Task<InvoiceResponse> ExecuteAsync(PayInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        var invoice = await invoices.GetByIdAsync(request.InvoiceId, request.OwnerScopeId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Invoice), request.InvoiceId);
        var source = await accounts.GetByIdAsync(request.AccountId, request.OwnerScopeId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Account), request.AccountId);
        var card = await accounts.GetByIdAsync(invoice.CreditCardAccountId, request.OwnerScopeId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Account), invoice.CreditCardAccountId);
        if (source.Type == AccountType.CreditCard)
            throw new OperationRejectedException("Selecione uma conta de saldo para pagar a fatura.");
        FinancialDates.ValidatePaymentDate(request.PaymentDate);

        var total = invoice.OpeningAmount + await transactions.GetInvoiceTotalAsync(invoice.Id, cancellationToken);
        invoice.Pay(request.Amount, total);
        var transfer = TransferGroup.Create(source, card, request.Amount, request.PaymentDate,
            invoice.OpeningAmount > 0 ? "Pagamento do saldo inicial do cartão" :
            $"Pagamento de fatura {invoice.PeriodMonth:00}/{invoice.PeriodYear}", invoice.Id);
        await transfers.AddAsync(transfer, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return InvoiceMapper.ToResponse(invoice, total);
    }
}
