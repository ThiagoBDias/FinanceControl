using FinanceControl.Application.DTOs;
using FinanceControl.Application.Interfaces;
using FinanceControl.Application.Validators;
using FinanceControl.Domain.Entities;
using FinanceControl.Domain.Enums;

namespace FinanceControl.Application.UseCases.Budgets;

public sealed class CreateBudgetUseCase : IUseCase<CreateBudgetRequest, BudgetResponse>
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBudgetUseCase(IBudgetRepository budgetRepository, ICategoryRepository categoryRepository, ITransactionRepository transactionRepository, IUnitOfWork unitOfWork)
    { _budgetRepository = budgetRepository; _categoryRepository = categoryRepository; _transactionRepository = transactionRepository; _unitOfWork = unitOfWork; }

    public async Task<BudgetResponse> ExecuteAsync(CreateBudgetRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, request.OwnerScopeId, cancellationToken)
            ?? throw new RequestValidationException(new[] { new ValidationError("CategoryId", $"Categoria {request.CategoryId} nao encontrada.") });
        if (!category.IsActive || category.Type != CategoryType.Expense)
            throw new RequestValidationException(new[] { new ValidationError("CategoryId", "O orcamento deve usar uma categoria de despesa ativa.") });
        if (await _budgetRepository.GetByCategoryMonthAsync(request.OwnerScopeId, request.CategoryId, request.Year, request.Month, cancellationToken) is not null)
            throw new RequestValidationException(new[] { new ValidationError("CategoryId", "Ja existe um orcamento para esta categoria neste mes.") });

        var budget = Budget.Create(request.OwnerScopeId, request.CategoryId, request.Year, request.Month, request.LimitAmount);
        await _budgetRepository.AddAsync(budget, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var spent = await _transactionRepository.GetExpenseTotalByCategoryAndMonthAsync(request.OwnerScopeId, request.CategoryId, request.Year, request.Month, cancellationToken);
        return BudgetMapper.ToResponse(budget, category.Name, spent);
    }

    private static void Validate(CreateBudgetRequest request)
    {
        var errors = new List<ValidationError>();
        if (request.OwnerScopeId == Guid.Empty) errors.Add(new ValidationError("OwnerScopeId", "OwnerScopeId e obrigatorio."));
        if (request.CategoryId == Guid.Empty) errors.Add(new ValidationError("CategoryId", "CategoryId e obrigatorio."));
        if (request.Year < 2000 || request.Year > 2100) errors.Add(new ValidationError("Year", "Ano invalido."));
        if (request.Month is < 1 or > 12) errors.Add(new ValidationError("Month", "Mes invalido."));
        if ((request.LimitAmount <= 0 || !FinanceControl.Domain.ValueObjects.Money.IsValid(request.LimitAmount))) errors.Add(new ValidationError("LimitAmount", "LimitAmount deve ser maior que zero."));
        if (errors.Count > 0) throw new RequestValidationException(errors);
    }
}
