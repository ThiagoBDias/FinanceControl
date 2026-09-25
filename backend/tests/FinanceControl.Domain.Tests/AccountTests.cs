using FinanceControl.Domain.Entities;
using FinanceControl.Domain.Enums;
using FinanceControl.Domain.Exceptions;
using FinanceControl.Domain.ValueObjects;

namespace FinanceControl.Domain.Tests;

public sealed class AccountTests
{
    private static readonly Guid Owner = Guid.NewGuid();

    [Fact]
    public void CreateAsset_WithNegativeInitialBalance_Throws()
    {
        Assert.Throws<InvalidMonetaryAmountException>(() =>
            Account.CreateAsset(Owner, "Conta", AccountType.Checking, -1m));
    }

    [Fact]
    public void RegisterIncome_IncreasesBalance()
    {
        var account = Account.CreateAsset(Owner, "Conta", AccountType.Checking, 100m);
        var category = Category.Create(Owner, "Salario", CategoryType.Income);

        account.RegisterIncome(500m, new DateOnly(2026, 9, 24), "Salario", category);

        Assert.Equal(600m, account.CalculatedBalance);
        Assert.Equal(account.CalculatedBalance, account.CachedBalance);
    }

    [Fact]
    public void RegisterExpense_DecreasesAssetBalance()
    {
        var account = Account.CreateAsset(Owner, "Conta", AccountType.Checking, 1000m);
        var category = Category.Create(Owner, "Mercado", CategoryType.Expense);

        account.RegisterExpense(250m, new DateOnly(2026, 9, 24), "Mercado", category);

        Assert.Equal(750m, account.CalculatedBalance);
        Assert.Equal(account.CalculatedBalance, account.CachedBalance);
    }

    [Fact]
    public void RegisterExpense_WithIncomeCategory_Throws()
    {
        var account = Account.CreateAsset(Owner, "Conta", AccountType.Checking, 1000m);
        var category = Category.Create(Owner, "Salario", CategoryType.Income);

        Assert.Throws<CategoryTypeMismatchException>(() =>
            account.RegisterExpense(100m, new DateOnly(2026, 9, 24), "Teste", category));
    }

    [Fact]
    public void CreditCardExpense_IncreasesLiabilityBalance()
    {
        var settings = CreditCardSettings.Create(3000m, 10, 20);
        var card = Account.CreateCreditCard(Owner, "Nubank", settings);
        var category = Category.Create(Owner, "Mercado", CategoryType.Expense);
        var invoiceId = Guid.NewGuid();

        card.RegisterExpense(180m, new DateOnly(2026, 9, 24), "Mercado", category, invoiceId: invoiceId);

        Assert.Equal(180m, card.CalculatedBalance);
        Assert.Equal(180m, card.CachedBalance);
    }

    [Fact]
    public void CreditCardExpense_WithoutInvoice_Throws()
    {
        var card = Account.CreateCreditCard(Owner, "Nubank", CreditCardSettings.Create(3000m, 10, 20));
        var category = Category.Create(Owner, "Mercado", CategoryType.Expense);

        Assert.Throws<InvoiceRequiredForCreditCardExpenseException>(() =>
            card.RegisterExpense(100m, new DateOnly(2026, 9, 24), "Mercado", category));
    }

    [Fact]
    public void RegisterExpense_WithInactiveCategory_Throws()
    {
        var account = Account.CreateAsset(Owner, "Conta", AccountType.Checking, 1000m);
        var category = Category.Create(Owner, "Mercado", CategoryType.Expense);
        category.Deactivate();

        Assert.Throws<InactiveCategoryException>(() =>
            account.RegisterExpense(100m, new DateOnly(2026, 9, 24), "Mercado", category));
    }

    [Fact]
    public void InactiveAccount_CannotReceiveExpense()
    {
        var account = Account.CreateAsset(Owner, "Conta", AccountType.Checking, 1000m);
        account.Deactivate();

        Assert.Throws<InactiveAccountException>(() =>
            account.RegisterExpense(100m, new DateOnly(2026, 9, 24), "Teste"));
    }
}
