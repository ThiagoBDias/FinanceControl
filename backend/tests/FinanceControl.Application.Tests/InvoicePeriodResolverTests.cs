using FinanceControl.Application.UseCases.Transactions;

namespace FinanceControl.Application.Tests;

public sealed class InvoicePeriodResolverTests
{
    [Fact]
    public void DateOnOrBeforeClosingDay_BelongsToCurrentMonth()
    {
        var (year, month) = InvoicePeriodResolver.ResolvePeriod(new DateOnly(2026, 9, 10), 10);
        var closing = InvoicePeriodResolver.ResolveClosingDate(year, month, 10);
        var due = InvoicePeriodResolver.ResolveDueDate(closing, 20);

        Assert.Equal(2026, year);
        Assert.Equal(9, month);
        Assert.Equal(new DateOnly(2026, 9, 10), closing);
        Assert.Equal(new DateOnly(2026, 9, 20), due);
    }

    [Fact]
    public void DateAfterClosingDay_BelongsToNextMonth()
    {
        var (year, month) = InvoicePeriodResolver.ResolvePeriod(new DateOnly(2026, 9, 11), 10);
        var closing = InvoicePeriodResolver.ResolveClosingDate(year, month, 10);
        var due = InvoicePeriodResolver.ResolveDueDate(closing, 20);

        Assert.Equal(2026, year);
        Assert.Equal(10, month);
        Assert.Equal(new DateOnly(2026, 10, 10), closing);
        Assert.Equal(new DateOnly(2026, 10, 20), due);
    }

    [Fact]
    public void ClosingDayBeyondMonthLength_IsClamped()
    {
        var (year, month) = InvoicePeriodResolver.ResolvePeriod(new DateOnly(2026, 2, 20), 31);
        var closing = InvoicePeriodResolver.ResolveClosingDate(year, month, 31);
        var due = InvoicePeriodResolver.ResolveDueDate(closing, 5);

        Assert.Equal(2026, year);
        Assert.Equal(2, month);
        Assert.Equal(new DateOnly(2026, 2, 28), closing);
        Assert.Equal(new DateOnly(2026, 3, 5), due);
    }
}
