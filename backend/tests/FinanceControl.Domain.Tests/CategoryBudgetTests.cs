using FinanceControl.Domain.Entities;
using FinanceControl.Domain.Enums;

namespace FinanceControl.Domain.Tests;

public sealed class CategoryBudgetTests
{
    [Fact]
    public void Category_NormalizesNameAndCanBeDeactivated()
    {
        var category = Category.Create(Guid.NewGuid(), "  Mercado  ", CategoryType.Expense);

        Assert.Equal("Mercado", category.Name);
        Assert.True(category.IsActive);

        category.Deactivate();

        Assert.False(category.IsActive);
    }

    [Fact]
    public void Budget_RejectsInvalidMonth()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Budget.Create(Guid.NewGuid(), Guid.NewGuid(), 2026, 13, 500m));
    }

    [Fact]
    public void Budget_ChangesLimit()
    {
        var budget = Budget.Create(Guid.NewGuid(), Guid.NewGuid(), 2026, 9, 800m);

        budget.ChangeLimit(950m);

        Assert.Equal(950m, budget.LimitAmount);
    }
}
