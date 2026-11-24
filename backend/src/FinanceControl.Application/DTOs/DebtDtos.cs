namespace FinanceControl.Application.DTOs;

public sealed record CreateDebtRequest(string Name, decimal TotalAmount, int InstallmentCount, DateOnly FirstDueDate, Guid? CategoryId);
public sealed record PayDebtInstallmentRequest(Guid AccountId, DateOnly PaymentDate);
public sealed record DebtInstallmentResponse(int Number, DateOnly DueDate, decimal Amount, DateOnly? PaymentDate);
public sealed record DebtResponse(Guid Id, string Name, decimal TotalAmount, decimal PendingAmount,
    Guid? CategoryId, IReadOnlyList<DebtInstallmentResponse> Installments);
