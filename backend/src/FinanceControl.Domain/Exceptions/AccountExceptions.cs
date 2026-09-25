namespace FinanceControl.Domain.Exceptions;

public sealed class InvalidCreditCardSettingsException : DomainException
{
    public InvalidCreditCardSettingsException(string message) : base(message)
    {
    }
}

public sealed class InactiveAccountException : DomainException
{
    public InactiveAccountException(Guid accountId)
        : base($"A conta '{accountId}' esta inativa e nao pode receber novos lancamentos.")
    {
    }
}

public sealed class InvoiceRequiredForCreditCardExpenseException : DomainException
{
    public InvoiceRequiredForCreditCardExpenseException(Guid accountId)
        : base($"Despesas na conta de cartao de credito '{accountId}' devem estar vinculadas a uma fatura (InvoiceId).")
    {
    }
}

public sealed class InvoiceNotAllowedForNonCreditCardAccountException : DomainException
{
    public InvoiceNotAllowedForNonCreditCardAccountException(Guid accountId)
        : base($"A conta '{accountId}' nao e do tipo CreditCard e nao pode ter despesas vinculadas a fatura.")
    {
    }
}
