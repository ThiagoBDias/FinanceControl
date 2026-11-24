using FinanceControl.Application.DTOs;
using FinanceControl.Application.Interfaces;
using FinanceControl.Application.Validators;

namespace FinanceControl.Application.UseCases.Budgets;

public sealed class UpdateBudgetUseCase : IUseCase<UpdateBudgetRequest, BudgetResponse>
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    public UpdateBudgetUseCase(IBudgetRepository budgetRepository, ICategoryRepository categoryRepository, ITransactionRepository transactionRepository, IUnitOfWork unitOfWork)
    { _budgetRepository = budgetRepository; _categoryRepository = categoryRepository; _transactionRepository = transactionRepository; _unitOfWork = unitOfWork; }

    public async Task<BudgetResponse> ExecuteAsync(UpdateBudgetRequest request, CancellationToken cancellationToken = default)
    {
        if (request.OwnerScopeId == Guid.Empty || request.BudgetId == Guid.Empty || (request.LimitAmount <= 0 || !FinanceControl.Domain.ValueObjects.Money.IsValid(request.LimitAmount)))
            throw new RequestValidationException(new[] { new ValidationError("Request", "OwnerScopeId, BudgetId e um limite maior que zero sao obrigatorios.") });
        var budget = await _budgetRepository.GetByIdAsync(request.BudgetId, request.OwnerScopeId, cancellationToken)
            ?? throw new RequestValidationException(new[] { new ValidationError("BudgetId", "Orcamento nao encontrado.") });
        budget.ChangeLimit(request.LimitAmount);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var category = await _categoryRepository.GetByIdAsync(budget.CategoryId, request.OwnerScopeId, cancellationToken);
        var spent = await _transactionRepository.GetExpenseTotalByCategoryAndMonthAsync(request.OwnerScopeId, budget.CategoryId, budget.Year, budget.Month, cancellationToken);
        return BudgetMapper.ToResponse(budget, category?.Name ?? "Categoria", spent);
    }
}
