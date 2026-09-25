using FinanceControl.Application.DTOs;
using FinanceControl.Application.Interfaces;
using FinanceControl.Application.Validators;
using FinanceControl.Domain.Enums;

namespace FinanceControl.Application.UseCases.Dashboard;

public sealed class GetDashboardUseCase : IUseCase<DashboardRequest, DashboardResponse>
{
    private readonly IAccountRepository _accounts;
    private readonly ICategoryRepository _categories;
    private readonly ITransactionRepository _transactions;
    private readonly IInvoiceRepository _invoices;
    private readonly IBudgetRepository _budgets;

    public GetDashboardUseCase(
        IAccountRepository accounts,
        ICategoryRepository categories,
        ITransactionRepository transactions,
        IInvoiceRepository invoices,
        IBudgetRepository budgets)
    {
        _accounts = accounts;
        _categories = categories;
        _transactions = transactions;
        _invoices = invoices;
        _budgets = budgets;
    }

    public async Task<DashboardResponse> ExecuteAsync(DashboardRequest request, CancellationToken cancellationToken = default)
    {
        if (request.OwnerScopeId == Guid.Empty)
            throw new RequestValidationException(new[] { new ValidationError(nameof(request.OwnerScopeId), "OwnerScopeId e obrigatorio.") });
        if (request.Year is < 2000 or > 2100)
            throw new RequestValidationException(new[] { new ValidationError(nameof(request.Year), "Ano invalido.") });
        if (request.Month is < 1 or > 12)
            throw new RequestValidationException(new[] { new ValidationError(nameof(request.Month), "Mes invalido.") });

        var accounts = await _accounts.ListByOwnerAsync(request.OwnerScopeId, cancellationToken);
        var categories = await _categories.ListByOwnerAsync(request.OwnerScopeId, cancellationToken);
        var categoryNames = categories.ToDictionary(c => c.Id, c => c.Name);

        var current = await _transactions.ListAsync(
            new ListTransactionsRequest(
                request.OwnerScopeId,
                CompetenceDateFrom: new DateOnly(request.Year, request.Month, 1),
                CompetenceDateTo: new DateOnly(request.Year, request.Month, DateTime.DaysInMonth(request.Year, request.Month))),
            cancellationToken);

        var income = current.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        var expenses = current.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

        var expensesByCategory = current
            .Where(t => t.Type == TransactionType.Expense)
            .GroupBy(t => t.CategoryId)
            .Select(g => new DashboardCategoryExpenseResponse(
                g.Key,
                g.Key is Guid categoryId && categoryNames.TryGetValue(categoryId, out var name) ? name : "Sem categoria",
                g.Sum(t => t.Amount)))
            .OrderByDescending(x => x.Amount)
            .ToList();

        var accountResponses = accounts
            .Select(a => new DashboardAccountResponse(a.Id, a.Name, a.Type, a.CalculatedBalance, a.Type == AccountType.CreditCard))
            .ToList();

        var netBalance = accounts.Sum(a => a.Type == AccountType.CreditCard ? -a.CalculatedBalance : a.CalculatedBalance);

        var cards = accounts
            .Where(a => a.Type == AccountType.CreditCard && a.CreditCardSettings is not null)
            .Select(a => new DashboardCardResponse(
                a.Id,
                a.Name,
                a.CreditCardSettings!.CreditLimit,
                Math.Max(0m, a.CalculatedBalance),
                Math.Max(0m, a.CreditCardSettings.CreditLimit - Math.Max(0m, a.CalculatedBalance))))
            .ToList();

        var invoiceResponses = new List<DashboardInvoiceResponse>();
        foreach (var card in cards)
        {
            var invoices = await _invoices.ListByCreditCardAccountAsync(card.AccountId, request.OwnerScopeId, cancellationToken);
            foreach (var invoice in invoices.Take(6))
            {
                var total = await _transactions.GetInvoiceTotalAsync(invoice.Id, cancellationToken);
                invoiceResponses.Add(new DashboardInvoiceResponse(
                    invoice.Id, invoice.CreditCardAccountId, invoice.PeriodYear, invoice.PeriodMonth, invoice.Status, total, invoice.DueDate));
            }
        }

        var budgets = await _budgets.ListByMonthAsync(request.OwnerScopeId, request.Year, request.Month, cancellationToken);
        var budgetResponses = new List<DashboardBudgetResponse>(budgets.Count);
        foreach (var budget in budgets)
        {
            var spent = await _transactions.GetExpenseTotalByCategoryAndMonthAsync(request.OwnerScopeId, budget.CategoryId, request.Year, request.Month, cancellationToken);
            var name = categoryNames.TryGetValue(budget.CategoryId, out var categoryName) ? categoryName : "Categoria";
            var remaining = budget.LimitAmount - spent;
            budgetResponses.Add(new DashboardBudgetResponse(budget.CategoryId, name, budget.LimitAmount, spent, remaining, spent > budget.LimitAmount));
        }

        var monthly = new List<DashboardMonthlyResponse>(6);
        for (var offset = 5; offset >= 0; offset--)
        {
            var date = new DateOnly(request.Year, request.Month, 1).AddMonths(-offset);
            var transactions = await _transactions.ListAsync(
                new ListTransactionsRequest(
                    request.OwnerScopeId,
                    CompetenceDateFrom: date,
                    CompetenceDateTo: date.AddMonths(1).AddDays(-1)),
                cancellationToken);
            var monthIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            var monthExpenses = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
            monthly.Add(new DashboardMonthlyResponse(date.Year, date.Month, monthIncome, monthExpenses, monthIncome - monthExpenses));
        }

        return new DashboardResponse(
            request.Year, request.Month, netBalance, income, expenses, income - expenses,
            expensesByCategory, accountResponses, cards, invoiceResponses, budgetResponses, monthly);
    }
}
