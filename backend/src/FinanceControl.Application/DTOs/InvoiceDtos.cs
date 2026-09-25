using FinanceControl.Domain.Entities;
using FinanceControl.Domain.Enums;

namespace FinanceControl.Application.DTOs;

public sealed record GetInvoiceRequest(Guid OwnerScopeId, Guid InvoiceId);

public sealed record ListInvoicesRequest(Guid OwnerScopeId, Guid CreditCardAccountId);

public sealed record CloseInvoiceRequest(Guid OwnerScopeId, Guid InvoiceId);


/// <summary>
/// TotalAmount NAO existe em Invoice no Domain (decisao da etapa anterior)
/// - e calculado aqui via ITransactionRepository.GetInvoiceTotalAsync.
/// </summary>
public sealed record InvoiceResponse(
    Guid Id,
    Guid OwnerScopeId,
    Guid CreditCardAccountId,
    int PeriodYear,
    int PeriodMonth,
    DateOnly ClosingDate,
    DateOnly DueDate,
    InvoiceStatus Status,
    decimal TotalAmount,
    DateTime CreatedAt,
    DateTime UpdatedAt);

internal static class InvoiceMapper
{
    public static InvoiceResponse ToResponse(Invoice invoice, decimal totalAmount) => new(
        invoice.Id, invoice.OwnerScopeId, invoice.CreditCardAccountId, invoice.PeriodYear, invoice.PeriodMonth,
        invoice.ClosingDate, invoice.DueDate, invoice.Status, totalAmount,
        invoice.CreatedAt, invoice.UpdatedAt);
}
