using FinanceControl.Domain.Exceptions;
using FinanceControl.Application.DTOs;
using FinanceControl.Application.Interfaces;
using FinanceControl.Application.Validators;
using FinanceControl.Domain.Entities;
using FinanceControl.Domain.Enums;
using FinanceControl.Domain.ValueObjects;

namespace FinanceControl.Application.UseCases.Transactions;

public sealed class RegisterExpenseUseCase(
    IAccountRepository accounts,
    ICategoryRepository categories,
    IInvoiceRepository invoices,
    IUnitOfWork unitOfWork,
    IValidator<RegisterExpenseRequest> validator) : IUseCase<RegisterExpenseRequest, TransactionResponse>
{
    public async Task<TransactionResponse> ExecuteAsync(RegisterExpenseRequest request, CancellationToken cancellationToken = default)
    {
        var validation = validator.Validate(request);
        if (!validation.IsValid)
            throw new RequestValidationException(validation.Errors);

        var account = await accounts.GetByIdAsync(request.AccountId, request.OwnerScopeId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Account), request.AccountId);
        var isCard = account.Type == AccountType.CreditCard;
        var count = request.InstallmentCount;
        if (count is < 1 or > 60 || request.Amount * 100 < count || (!isCard && count > 1))
            throw new OperationRejectedException("Informe de 1 a 60 parcelas no cartão, com pelo menos um centavo por parcela.");
        if (request.CompetenceDate.Year is < 1900 or > 2200)
            throw new OperationRejectedException("Informe uma data de competência válida.");
        if (request.DueDate?.Year is < 1900 or > 2200)
            throw new OperationRejectedException("Informe uma data de vencimento válida.");
        if (request.Scheduled && (isCard || request.DueDate is null))
            throw new OperationRejectedException("Uma conta a pagar exige vencimento e uma conta de saldo.");

        if (!isCard && !request.Scheduled)
            FinancialDates.ValidatePaymentDate(request.CompetenceDate);

        Category? category = null;
        if (request.CategoryId is Guid categoryId)
            category = await categories.GetByIdAsync(categoryId, request.OwnerScopeId, cancellationToken)
                ?? throw new EntityNotFoundException(nameof(Category), categoryId);

        var purchaseId = count > 1 ? Guid.NewGuid() : (Guid?)null;
        Transaction? first = null;
        // Advance from the original date and invoice period so short months don't shift the schedule.
        var initialPeriod = isCard
            ? InvoicePeriodResolver.ResolvePeriod(request.CompetenceDate, account.CreditCardSettings!.ClosingDay)
            : default;

        for (var number = 1; number <= count; number++)
        {
            var competence = request.CompetenceDate.AddMonths(number - 1);
            var amount = Money.Installment(request.Amount, count, number);
            var dueDate = request.DueDate;
            Guid? invoiceId = null;

            if (isCard)
            {
                var period = new DateOnly(initialPeriod.Year, initialPeriod.Month, 1).AddMonths(number - 1);
                var invoice = await GetOrOpenInvoiceAsync(account, period, cancellationToken);
                invoiceId = invoice.Id;
                dueDate = invoice.DueDate;
            }

            var transaction = account.RegisterExpense(amount, competence, request.Description, category, dueDate,
                invoiceId, request.Scheduled, purchaseId, number, count);
            if (!isCard && !request.Scheduled)
                transaction.MarkAsPaid(competence);
            first ??= transaction;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return TransactionMapper.ToResponse(first!);
    }

    private async Task<Invoice> GetOrOpenInvoiceAsync(Account account, DateOnly period, CancellationToken cancellationToken)
    {
        var invoice = await invoices.GetPeriodInvoiceAsync(account.Id, period.Year, period.Month, cancellationToken);
        if (invoice is not null)
        {
            if (invoice.Status != InvoiceStatus.Open)
                throw new OperationRejectedException("Já existe uma fatura fechada para a data informada.");
            return invoice;
        }

        var settings = account.CreditCardSettings!;
        var closing = InvoicePeriodResolver.ResolveClosingDate(period.Year, period.Month, settings.ClosingDay);
        var due = InvoicePeriodResolver.ResolveDueDate(closing, settings.DueDay);
        invoice = Invoice.Open(account.OwnerScopeId, account.Id, period.Year, period.Month, closing, due);
        await invoices.AddAsync(invoice, cancellationToken);
        return invoice;
    }
}
