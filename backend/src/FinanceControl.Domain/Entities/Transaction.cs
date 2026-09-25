using FinanceControl.Domain.Enums;
using FinanceControl.Domain.Exceptions;

namespace FinanceControl.Domain.Entities;

/// <summary>
/// Lancamento financeiro. Representa Income, Expense ou uma perna de
/// transferencia (TransferOut/TransferIn). O valor (Amount) e sempre
/// armazenado positivo (regra 2) - a natureza da movimentacao vem do Type.
///
/// So pode ser criada por Account (RegisterExpense/RegisterIncome) ou por
/// TransferGroup (atraves de Account.RegisterTransferOut/In) - o construtor
/// e as factories sao internal para impedir que a Application layer crie
/// uma Transaction "solta", contornando os invariantes.
/// </summary>
public sealed class Transaction
{
    public Guid Id { get; }
    public Guid OwnerScopeId { get; }
    public Guid AccountId { get; }
    public Guid? CategoryId { get; }
    public Guid? InvoiceId { get; }
    public Guid? TransferGroupId { get; }
    public TransactionType Type { get; }
    public decimal Amount { get; }
    public string Description { get; }
    public DateOnly CompetenceDate { get; }
    public DateOnly? DueDate { get; }
    public DateOnly? PaymentDate { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    // Reservados para futura deduplicacao/integracao com Open Finance.
    // Nenhuma logica de deduplicacao ou importacao foi implementada nesta
    // etapa - os campos existem apenas para nao exigir migration depois
    // (ja fazem parte da arquitetura aprovada).
    public string? Provider { get; }
    public string? ExternalAccountId { get; }
    public string? ExternalTransactionId { get; }
    public string? DedupHash { get; }

    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; private set; }

    private Transaction(
        Guid id,
        Guid ownerScopeId,
        Guid accountId,
        Guid? categoryId,
        Guid? invoiceId,
        Guid? transferGroupId,
        TransactionType type,
        decimal amount,
        string? description,
        DateOnly competenceDate,
        DateOnly? dueDate,
        DateTime createdAt)
    {
        ValidateAmount(amount);
        ValidateTransferConsistency(type, transferGroupId);
        ValidateCategoryConsistency(type, categoryId);

        Id = id;
        OwnerScopeId = ownerScopeId;
        AccountId = accountId;
        CategoryId = categoryId;
        InvoiceId = invoiceId;
        TransferGroupId = transferGroupId;
        Type = type;
        Amount = amount;
        Description = (description ?? string.Empty).Trim();
        CompetenceDate = competenceDate;
        DueDate = dueDate;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    /// <summary>Cria um lancamento simples: Income ou Expense.</summary>
    internal static Transaction CreateSimple(
        Guid ownerScopeId,
        Guid accountId,
        Guid? categoryId,
        Guid? invoiceId,
        TransactionType type,
        decimal amount,
        string? description,
        DateOnly competenceDate,
        DateOnly? dueDate)
    {
        return new Transaction(
            Guid.NewGuid(), ownerScopeId, accountId, categoryId, invoiceId, transferGroupId: null,
            type, amount, description, competenceDate, dueDate, DateTime.UtcNow);
    }

    /// <summary>Cria uma perna (leg) de transferencia: TransferOut ou TransferIn.</summary>
    internal static Transaction CreateTransferLeg(
        Guid ownerScopeId,
        Guid accountId,
        TransactionType type,
        decimal amount,
        string? description,
        DateOnly competenceDate,
        Guid transferGroupId)
    {
        return new Transaction(
            Guid.NewGuid(), ownerScopeId, accountId, categoryId: null, invoiceId: null, transferGroupId,
            type, amount, description, competenceDate, dueDate: null, DateTime.UtcNow);
    }

    public void MarkAsPaid(DateOnly paymentDate)
    {
        PaymentDate = paymentDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Delete()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void ValidateAmount(decimal amount)
    {
        if (amount <= 0)
            throw new InvalidMonetaryAmountException(amount);
    }

    private static void ValidateTransferConsistency(TransactionType type, Guid? transferGroupId)
    {
        var isTransferType = type is TransactionType.TransferIn or TransactionType.TransferOut;

        if (isTransferType && transferGroupId is null)
            throw new TransferGroupRequiredException();

        if (!isTransferType && transferGroupId is not null)
            throw new TransferGroupNotAllowedException();
    }

    private static void ValidateCategoryConsistency(TransactionType type, Guid? categoryId)
    {
        var isTransferType = type is TransactionType.TransferIn or TransactionType.TransferOut;

        if (isTransferType && categoryId is not null)
            throw new CategoryNotAllowedForTransferException();
    }
}
