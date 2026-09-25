namespace FinanceControl.Domain.Enums;

/// <summary>
/// Tipo da conta financeira. Determina a natureza contábil (Asset/Liability)
/// utilizada pelo calculo de saldo em Account.
/// </summary>
public enum AccountType
{
    Checking = 1,
    Savings = 2,
    Cash = 3,
    CreditCard = 4,

    /// <summary>
    /// Reservado para uso futuro. Nenhuma entidade ou logica de investimento
    /// foi implementada nesta etapa - apenas o valor do enum, para nao exigir
    /// migration quando o modulo de investimentos for criado.
    /// </summary>
    Investment = 5
}
