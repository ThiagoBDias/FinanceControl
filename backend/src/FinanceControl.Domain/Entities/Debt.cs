using FinanceControl.Domain.Exceptions;
using FinanceControl.Domain.ValueObjects;

namespace FinanceControl.Domain.Entities;

public sealed class Debt
{
    private readonly List<DebtInstallment> _installments = new();
    public Guid Id { get; private set; }
    public Guid OwnerScopeId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal TotalAmount { get; private set; }
    public Guid? CategoryId { get; private set; }
    public IReadOnlyCollection<DebtInstallment> Installments => _installments.AsReadOnly();

    public static Debt Create(Guid owner, string name, decimal total, int count, DateOnly firstDueDate, Guid? categoryId)
    {
        var debt = new Debt
        {
            Id = Guid.NewGuid(), OwnerScopeId = owner, Name = name, TotalAmount = total, CategoryId = categoryId
        };
        for (var number = 1; number <= count; number++)
        {
            debt._installments.Add(new DebtInstallment(debt.Id, number,
                firstDueDate.AddMonths(number - 1), Money.Installment(total, count, number)));
        }
        return debt;
    }
}

public sealed class DebtInstallment(Guid debtId, int number, DateOnly dueDate, decimal amount)
{
    public Guid DebtId { get; } = debtId;
    public int Number { get; } = number;
    public DateOnly DueDate { get; } = dueDate;
    public decimal Amount { get; } = amount;
    public DateOnly? PaymentDate { get; private set; }
    public Guid? TransactionId { get; private set; }

    public void MarkPaid(Transaction transaction, DateOnly date)
    {
        if (PaymentDate is not null)
            throw new OperationRejectedException("Esta parcela já foi paga.");
        TransactionId = transaction.Id;
        PaymentDate = date;
    }
}
