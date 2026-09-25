using FinanceControl.Application.DTOs;
using FinanceControl.Application.Interfaces;
using FinanceControl.Application.Validators;
using FinanceControl.Domain.Entities;

namespace FinanceControl.Application.UseCases.Transactions;

public sealed class RegisterIncomeUseCase : IUseCase<RegisterIncomeRequest, TransactionResponse>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<RegisterIncomeRequest> _validator;

    public RegisterIncomeUseCase(
        IAccountRepository accountRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        IValidator<RegisterIncomeRequest> validator)
    {
        _accountRepository = accountRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<TransactionResponse> ExecuteAsync(RegisterIncomeRequest request, CancellationToken cancellationToken = default)
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

        var transaction = account.RegisterIncome(
            request.Amount, request.CompetenceDate, request.Description, category, request.DueDate);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return TransactionMapper.ToResponse(transaction);
    }
}
