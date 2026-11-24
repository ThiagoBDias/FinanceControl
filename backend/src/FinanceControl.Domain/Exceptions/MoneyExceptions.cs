namespace FinanceControl.Domain.Exceptions;

/// <summary>
/// Lancada quando um valor monetario informado nao e estritamente positivo.
/// Valores monetarios armazenados sao sempre positivos (regra 2 do MVP).
/// </summary>
public sealed class InvalidMonetaryAmountException : DomainException
{
    public InvalidMonetaryAmountException(decimal amount)
        : base($"Valor monetário inválido. Informe até duas casas decimais dentro do intervalo permitido. Valor informado: {amount}.")
    {
    }
}
