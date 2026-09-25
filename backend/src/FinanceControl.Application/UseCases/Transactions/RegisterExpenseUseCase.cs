using FinanceControl.Application.DTOs;
using FinanceControl.Application.Interfaces;
using FinanceControl.Application.Validators;
using FinanceControl.Domain.Entities;
using FinanceControl.Domain.Enums;

namespace FinanceControl.Application.UseCases.Transactions;

/// <summary>
/// Regra 8: compra no cartao e Expense e pertence a fatura correspondente.
/// O chamador nao informa InvoiceId - se a conta for CreditCard, este caso
/// de uso encontra a fatura em aberto do periodo ou abre uma nova
/// automaticamente (ver InvoicePeriodResolver). Para contas comuns,
/// InvoiceId permanece null - o proprio Account.RegisterExpense recusa a
/// combinacao errada (InvoiceRequiredForCreditCardExpenseException /
/// InvoiceNotAllowedForNonCreditCardAccountException).
/// </summary>
public sealed class RegisterExpenseUseCase : IUseCase<RegisterExpenseRequest, TransactionResponse>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<RegisterExpenseRequest> _validator;

    public RegisterExpenseUseCase(
        IAccountRepository accountRepository,
        ICategoryRepository categoryRepository,
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork,
        IValidator<RegisterExpenseRequest> validator)
    {
        _accountRepository = accountRepository;
        _categoryRepository = categoryRepository;
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<TransactionResponse> ExecuteAsync(RegisterExpenseRequest request, CancellationToken cancellationToken = default)
    {
        var validation = _validator.Validate(request);
        if (!validation.IsValid)
            throw new RequestValidationException(validation.Errors);

        var account = await _accountRepository.GetByIdAsync(request.AccountId, request.OwnerScopeId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Account), request.AccountId);

        Category? category = null;
        if (request.CategoryId is Guid categoryId)
        {
            category = await _categoryRepository.GetByIdAsync(categoryId, request.OwnerScopeId, cancellationToken)
                ?? throw new EntityNotFoundException(nameof(Category), categoryId);
        }

        Guid? invoiceId = null;
        var dueDate = request.DueDate;

        if (account.Type == AccountType.CreditCard)
        {
            var settings = account.CreditCardSettings!; // Account.CreateCreditCard garante que nunca e null aqui.
            var (year, month) = InvoicePeriodResolver.ResolvePeriod(request.CompetenceDate, settings.ClosingDay);

            var invoice = await _invoiceRepository.GetOpenInvoiceAsync(account.Id, year, month, cancellationToken);
            if (invoice is null)
            {
                var closingDate = InvoicePeriodResolver.ResolveClosingDate(year, month, settings.ClosingDay);
                var invoiceDueDate = InvoicePeriodResolver.ResolveDueDate(closingDate, settings.DueDay);

                invoice = Invoice.Open(request.OwnerScopeId, account.Id, year, month, closingDate, invoiceDueDate);
                await _invoiceRepository.AddAsync(invoice, cancellationToken);
            }

            invoiceId = invoice.Id;
            dueDate = invoice.DueDate;
        }

        var transaction = account.RegisterExpense(
            request.Amount, request.CompetenceDate, request.Description, category, dueDate, invoiceId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return TransactionMapper.ToResponse(transaction);
    }
}
