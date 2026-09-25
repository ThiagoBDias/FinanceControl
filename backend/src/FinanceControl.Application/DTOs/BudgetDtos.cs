using FinanceControl.Domain.Entities;

namespace FinanceControl.Application.DTOs;

public sealed record CreateBudgetRequest(Guid OwnerScopeId, Guid CategoryId, int Year, int Month, decimal LimitAmount);
public sealed record UpdateBudgetRequest(Guid OwnerScopeId, Guid BudgetId, decimal LimitAmount);
public sealed record ListBudgetsRequest(Guid OwnerScopeId, int Year, int Month);

public sealed record BudgetResponse(
    Guid Id,
    Guid OwnerScopeId,
    Guid CategoryId,
    string CategoryName,
    int Year,
    int Month,
    decimal LimitAmount,
    decimal SpentAmount,
    decimal RemainingAmount,
    bool IsOverLimit,
    DateTime CreatedAt,
    DateTime UpdatedAt);

internal static class BudgetMapper
{
    public static BudgetResponse ToResponse(Budget budget, string categoryName, decimal spentAmount)
    {
        var remaining = budget.LimitAmount - spentAmount;
        return new BudgetResponse(
            budget.Id, budget.OwnerScopeId, budget.CategoryId, categoryName,
            budget.Year, budget.Month, budget.LimitAmount, spentAmount, remaining,
            spentAmount > budget.LimitAmount, budget.CreatedAt, budget.UpdatedAt);
    }
}
