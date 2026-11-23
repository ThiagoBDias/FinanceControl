using FinanceControl.Application.DTOs;
using FinanceControl.Application.Interfaces;
using FinanceControl.Application.Validators;
using FinanceControl.Domain.Entities;
using FinanceControl.Domain.ValueObjects;

namespace FinanceControl.Application.UseCases.CreditCards;

public sealed class CreateCreditCardUseCase : IUseCase<CreateCreditCardRequest, AccountResponse>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IInvoiceRepository _invoices;
    private readonly IValidator<CreateCreditCardRequest> _validator;

    public CreateCreditCardUseCase(
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        IInvoiceRepository invoices,
        IValidator<CreateCreditCardRequest> validator)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _invoices = invoices;
    }

    public async Task<AccountResponse> ExecuteAsync(CreateCreditCardRequest request, CancellationToken cancellationToken = default)
    {
        var validation = _validator.Validate(request);
        if (!validation.IsValid)
            throw new RequestValidationException(validation.Errors);

        var settings = CreditCardSettings.Create(request.CreditLimit, request.ClosingDay, request.DueDay);
        var account = Account.CreateCreditCard(request.OwnerScopeId, request.Name, settings, request.InitialBalance);

        await _accountRepository.AddAsync(account, cancellationToken);
        if (account.InitialBalance > 0)
            await _invoices.AddAsync(Invoice.ForOpeningBalance(account, FinancialDates.Today), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return AccountMapper.ToResponse(account);
    }
}
