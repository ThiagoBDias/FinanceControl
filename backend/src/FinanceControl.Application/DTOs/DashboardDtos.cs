using FinanceControl.Domain.Enums;

namespace FinanceControl.Application.DTOs;

public sealed record DashboardRequest(Guid OwnerScopeId, int Year, int Month);

public sealed record DashboardCategoryExpenseResponse(Guid? CategoryId, string CategoryName, decimal Amount);

public sealed record DashboardAccountResponse(Guid AccountId, string Name, AccountType Type, decimal Balance, bool IsLiability);

public sealed record DashboardCardResponse(Guid AccountId, string Name, decimal CreditLimit, decimal UsedAmount, decimal AvailableAmount);

public sealed record DashboardMonthlyResponse(int Year, int Month, decimal Income, decimal Expenses, decimal Result);

public sealed record DashboardInvoiceResponse(Guid Id, Guid CreditCardAccountId, int PeriodYear, int PeriodMonth, InvoiceStatus Status, decimal TotalAmount, decimal PendingAmount, DateOnly DueDate);

public sealed record DashboardBudgetResponse(Guid CategoryId, string CategoryName, decimal LimitAmount, decimal SpentAmount, decimal RemainingAmount, bool IsOverLimit);

public sealed record DashboardCommitment(string Kind, string Name, DateOnly DueDate, decimal Amount);

public sealed record DashboardResponse(
    int Year,
    int Month,
    decimal NetBalance,
    decimal CashBalance,
    decimal Income,
    decimal Expenses,
    decimal Result,
    IReadOnlyList<DashboardCategoryExpenseResponse> ExpensesByCategory,
    IReadOnlyList<DashboardAccountResponse> Accounts,
    IReadOnlyList<DashboardCardResponse> CreditCards,
    IReadOnlyList<DashboardInvoiceResponse> Invoices,
    IReadOnlyList<DashboardBudgetResponse> Budgets,
    IReadOnlyList<DashboardMonthlyResponse> MonthlyEvolution,
    IReadOnlyList<DashboardCommitment> Upcoming,
    IReadOnlyList<DashboardMonthlyCommitment> ProjectedCommitments);

public sealed record DashboardMonthlyCommitment(int Year, int Month, decimal Amount);
