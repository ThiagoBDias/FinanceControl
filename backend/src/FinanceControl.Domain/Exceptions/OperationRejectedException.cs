namespace FinanceControl.Domain.Exceptions;

public sealed class OperationRejectedException(string message) : DomainException(message);
