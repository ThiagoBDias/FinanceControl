namespace FinanceControl.Domain.Enums;

/// <summary>
/// Natureza do lancamento. O efeito sobre o saldo da conta depende
/// tambem do AccountType (ver Account.SignedEffect).
/// </summary>
public enum TransactionType
{
    Income = 1,
    Expense = 2,
    TransferOut = 3,
    TransferIn = 4
}
