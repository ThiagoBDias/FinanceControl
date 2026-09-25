namespace FinanceControl.Domain.Exceptions;

/// <summary>
/// Excecao base para toda violacao de regra de negocio do Domain.
/// Nunca deve ser lancada diretamente - use uma subclasse especifica.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }
}
