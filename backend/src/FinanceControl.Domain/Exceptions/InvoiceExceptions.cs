using FinanceControl.Domain.Enums;

namespace FinanceControl.Domain.Exceptions;

public sealed class InvalidInvoiceStatusTransitionException : DomainException
{
    public InvalidInvoiceStatusTransitionException(InvoiceStatus current, InvoiceStatus target)
        : base($"Nao e possivel mover a fatura do status '{current}' para '{target}'.")
    {
    }
}
