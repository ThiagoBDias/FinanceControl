namespace FinanceControl.Domain.Exceptions;

/// <summary>
/// Lancada quando um valor monetario informado nao e estritamente positivo.
/// Valores monetarios armazenados sao sempre positivos (regra 2 do MVP).
/// </summary>
public sealed class InvalidMonetaryAmountException : DomainException
{
    public InvalidMonetaryAmountException(decimal amount)
        : base($"O valor monetario deve ser maior que zero. Valor informado: {amount}.")
    {
    }
}
