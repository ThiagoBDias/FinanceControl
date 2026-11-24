using FinanceControl.Domain.Exceptions;
using FinanceControl.Domain.Entities;
using FinanceControl.Domain.Enums;
using FinanceControl.Domain.ValueObjects;

namespace FinanceControl.Domain.Tests;

public sealed class MoneyTests
{
    [Fact]
    public void Installments_DistributeResidualCentsWithoutLosingValue()
    {
        var amounts = Enumerable.Range(1, 3).Select(number => Money.Installment(100m, 3, number)).ToArray();
        Assert.Equal(new[] { 33.34m, 33.33m, 33.33m }, amounts);
        Assert.Equal(100m, amounts.Sum());
        Assert.False(Money.IsValid(0.001m));
    }

    [Fact]
    public void ScheduledExpense_OnlyAffectsBalanceAfterPayment()
    {
        var account = Account.CreateAsset(Guid.NewGuid(), "Conta", AccountType.Checking, 100m);
        var date = new DateOnly(2026, 11, 20);
        var expense = account.RegisterExpense(20m, date, "Despesa", dueDate: date, scheduled: true);
        Assert.True(expense.IsPending);
        Assert.Equal(100m, account.CalculatedBalance);

        account.SettleExpense(expense, date);
        Assert.False(expense.IsPending);
        Assert.Equal(80m, account.CalculatedBalance);
        Assert.Equal(80m, account.CachedBalance);
        Assert.Throws<OperationRejectedException>(() => account.SettleExpense(expense, date));
    }
}
