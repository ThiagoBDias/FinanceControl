namespace FinanceControl.Domain.Exceptions;

public sealed class InvalidEmailException : DomainException
{
    public InvalidEmailException(string email)
        : base($"O e-mail informado e invalido: '{email}'.")
    {
    }
}
