using FinanceControl.Domain.Entities;
using FinanceControl.Domain.Enums;

namespace FinanceControl.Application.DTOs;

public sealed record CreateAccountRequest(Guid OwnerScopeId, string Name, AccountType Type, decimal InitialBalance);

public sealed record GetAccountRequest(Guid OwnerScopeId, Guid AccountId);

public sealed record ListAccountsRequest(Guid OwnerScopeId);

/// <summary>Unica alteracao suportada e o nome - Type, saldo inicial e CreditCardSettings sao imutaveis apos a criacao.</summary>
public sealed record UpdateAccountRequest(Guid OwnerScopeId, Guid AccountId, string Name);

public sealed record DeactivateAccountRequest(Guid OwnerScopeId, Guid AccountId);

public sealed record CreditCardSettingsResponse(decimal CreditLimit, int ClosingDay, int DueDay);

public sealed record AccountResponse(
    Guid Id,
    Guid OwnerScopeId,
    string Name,
    AccountType Type,
    decimal InitialBalance,
    decimal CalculatedBalance,
    bool IsActive,
    CreditCardSettingsResponse? CreditCardSettings,
    DateTime CreatedAt,
    DateTime UpdatedAt);

internal static class AccountMapper
{
    public static AccountResponse ToResponse(Account account) => new(
        account.Id,
        account.OwnerScopeId,
        account.Name,
        account.Type,
        account.InitialBalance,
        account.CalculatedBalance,
        account.IsActive,
        account.CreditCardSettings is null
            ? null
            : new CreditCardSettingsResponse(
                account.CreditCardSettings.CreditLimit,
                account.CreditCardSettings.ClosingDay,
                account.CreditCardSettings.DueDay),
        account.CreatedAt,
        account.UpdatedAt);
}
