namespace FinanceControl.Application.DTOs;

/// <summary>
/// Casos de uso de cartao usam AccountResponse como retorno (CreditCard
/// e um Account com Type = CreditCard) - so as requisicoes sao especificas.
/// </summary>
public sealed record CreateCreditCardRequest(
    Guid OwnerScopeId,
    string Name,
    decimal CreditLimit,
    int ClosingDay,
    int DueDay,
    decimal InitialBalance);

public sealed record GetCreditCardRequest(Guid OwnerScopeId, Guid AccountId);

public sealed record ListCreditCardsRequest(Guid OwnerScopeId);
