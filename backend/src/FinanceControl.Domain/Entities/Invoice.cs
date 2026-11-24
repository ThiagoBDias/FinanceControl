using FinanceControl.Domain.Enums;
using FinanceControl.Domain.Exceptions;

namespace FinanceControl.Domain.Entities;

/// <summary>
/// Fatura de cartao de credito. Controla apenas o ciclo de vida
/// (Open -> Closed).
///
/// Decisao explicita: esta entidade NAO guarda TotalAmount. O total da
/// fatura e sempre a soma das Transactions (Type = Expense) vinculadas a
/// ela via InvoiceId - sera calculado por query na Application/
/// Infrastructure quando a persistencia existir. Guardar TotalAmount aqui
/// exigiria sincronizar dois aggregates (Account e Invoice) dentro do
/// Domain, o que preferimos evitar nesta etapa (ver secao de decisoes).
/// </summary>
public sealed class Invoice
{
    public Guid Id { get; }
    public Guid OwnerScopeId { get; }
    public Guid CreditCardAccountId { get; }
    public int PeriodYear { get; }
    public int PeriodMonth { get; }
    public DateOnly ClosingDate { get; }
    public DateOnly DueDate { get; }
    public InvoiceStatus Status { get; private set; }
    public decimal OpeningAmount { get; private set; }
    public decimal PaidAmount { get; private set; }
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; private set; }

    private Invoice(
        Guid id,
        Guid ownerScopeId,
        Guid creditCardAccountId,
        int periodYear,
        int periodMonth,
        DateOnly closingDate,
        DateOnly dueDate,
        DateTime createdAt)
    {
        Id = id;
        OwnerScopeId = ownerScopeId;
        CreditCardAccountId = creditCardAccountId;
        PeriodYear = periodYear;
        PeriodMonth = periodMonth;
        ClosingDate = closingDate;
        DueDate = dueDate;
        Status = InvoiceStatus.Open;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public static Invoice Open(
        Guid ownerScopeId,
        Guid creditCardAccountId,
        int periodYear,
        int periodMonth,
        DateOnly closingDate,
        DateOnly dueDate)
    {
        if (periodMonth is < 1 or > 12)
            throw new ArgumentOutOfRangeException(nameof(periodMonth), "O mes de referencia deve estar entre 1 e 12.");

        return new Invoice(Guid.NewGuid(), ownerScopeId, creditCardAccountId, periodYear, periodMonth, closingDate, dueDate, DateTime.UtcNow);
    }

    public static Invoice ForOpeningBalance(Account card, DateOnly date)
    {
        var invoice = Open(card.OwnerScopeId, card.Id, 1, 1, date, date);
        invoice.OpeningAmount = card.InitialBalance;
        invoice.Close();
        return invoice;
    }

    /// <summary>Fecha a fatura (deixa de aceitar novas compras no periodo).</summary>
    public void Close()
    {
        EnsureTransition(InvoiceStatus.Closed);
        Status = InvoiceStatus.Closed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Pay(decimal amount, decimal total)
    {
        if (amount <= 0 || !FinanceControl.Domain.ValueObjects.Money.IsValid(amount) || amount > total - PaidAmount)
            throw new OperationRejectedException("O pagamento deve ser positivo e não pode superar o saldo da fatura.");
        if (Status != InvoiceStatus.Closed)
            throw new OperationRejectedException("Feche a fatura antes de registrar o pagamento.");
        PaidAmount += amount;
        UpdatedAt = DateTime.UtcNow;
    }

    private void EnsureTransition(InvoiceStatus target)
    {
        var isValid = (Status, target) switch
        {
            (InvoiceStatus.Open, InvoiceStatus.Closed) => true,
            _ => false
        };

        if (!isValid)
            throw new InvalidInvoiceStatusTransitionException(Status, target);
    }
}
