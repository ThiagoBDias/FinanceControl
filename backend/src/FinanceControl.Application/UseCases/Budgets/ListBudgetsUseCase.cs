using FinanceControl.Application.DTOs;
using FinanceControl.Application.Interfaces;
using FinanceControl.Application.Validators;

namespace FinanceControl.Application.UseCases.Budgets;

public sealed class ListBudgetsUseCase : IUseCase<ListBudgetsRequest, IReadOnlyList<BudgetResponse>>
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITransactionRepository _transactionRepository;
    public ListBudgetsUseCase(IBudgetRepository budgetRepository, ICategoryRepository categoryRepository, ITransactionRepository transactionRepository)
    { _budgetRepository = budgetRepository; _categoryRepository = categoryRepository; _transactionRepository = transactionRepository; }

    public async Task<IReadOnlyList<BudgetResponse>> ExecuteAsync(ListBudgetsRequest request, CancellationToken cancellationToken = default)
    {
        if (request.OwnerScopeId == Guid.Empty || request.Year < 2000 || request.Year > 2100 || request.Month is < 1 or > 12)
            throw new RequestValidationException(new[] { new ValidationError("Request", "OwnerScopeId, ano e mes validos sao obrigatorios.") });
        var budgets = await _budgetRepository.ListByMonthAsync(request.OwnerScopeId, request.Year, request.Month, cancellationToken);
        var result = new List<BudgetResponse>(budgets.Count);
        foreach (var budget in budgets)
        {
            var category = await _categoryRepository.GetByIdAsync(budget.CategoryId, request.OwnerScopeId, cancellationToken);
            var spent = await _transactionRepository.GetExpenseTotalByCategoryAndMonthAsync(request.OwnerScopeId, budget.CategoryId, request.Year, request.Month, cancellationToken);
            result.Add(BudgetMapper.ToResponse(budget, category?.Name ?? "Categoria", spent));
        }
        return result;
    }
}
