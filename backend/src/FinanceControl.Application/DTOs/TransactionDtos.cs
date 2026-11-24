using FinanceControl.Domain.Entities;
using FinanceControl.Domain.Enums;

namespace FinanceControl.Application.DTOs;

public sealed record RegisterIncomeRequest(
    Guid OwnerScopeId,
    Guid AccountId,
    decimal Amount,
    DateOnly CompetenceDate,
    string Description,
    Guid? CategoryId,
    DateOnly? DueDate);

/// <summary>
/// InvoiceId NAO faz parte da requisicao: quando a conta e do tipo
/// CreditCard, o caso de uso resolve/abre a fatura do periodo
/// automaticamente (ver RegisterExpenseUseCase). O chamador so informa
/// a conta e a data de competencia.
/// </summary>
public sealed record RegisterExpenseRequest(
    Guid OwnerScopeId,
    Guid AccountId,
    decimal Amount,
    DateOnly CompetenceDate,
    string Description,
    Guid? CategoryId,
    DateOnly? DueDate,
    int InstallmentCount = 1,
    bool Scheduled = false);

public sealed record GetTransactionRequest(Guid OwnerScopeId, Guid TransactionId);

public sealed record ListTransactionsRequest(
    Guid OwnerScopeId,
    Guid? AccountId = null,
    Guid? CategoryId = null,
    Guid? InvoiceId = null,
    TransactionType? Type = null,
    DateOnly? CompetenceDateFrom = null,
    DateOnly? CompetenceDateTo = null);

public sealed record TransactionResponse(
    Guid Id,
    Guid OwnerScopeId,
    Guid AccountId,
    Guid? CategoryId,
    Guid? InvoiceId,
    Guid? TransferGroupId,
    TransactionType Type,
    decimal Amount,
    string Description,
    DateOnly CompetenceDate,
    DateOnly? DueDate,
    DateOnly? PaymentDate,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    bool IsPending,
    Guid? PurchaseId,
    int? InstallmentNumber,
    int? InstallmentCount);

internal static class TransactionMapper
{
    public static TransactionResponse ToResponse(Transaction transaction) => new(
        transaction.Id, transaction.OwnerScopeId, transaction.AccountId, transaction.CategoryId,
        transaction.InvoiceId, transaction.TransferGroupId, transaction.Type, transaction.Amount,
        transaction.Description, transaction.CompetenceDate, transaction.DueDate, transaction.PaymentDate,
        transaction.CreatedAt, transaction.UpdatedAt, transaction.IsPending,
        transaction.PurchaseId, transaction.InstallmentNumber, transaction.InstallmentCount);
}
