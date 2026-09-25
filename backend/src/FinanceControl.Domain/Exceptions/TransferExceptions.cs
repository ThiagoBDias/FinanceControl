namespace FinanceControl.Domain.Exceptions;

public sealed class TransferToSameAccountException : DomainException
{
    public TransferToSameAccountException()
        : base("Nao e possivel transferir de uma conta para ela mesma.")
    {
    }
}

public sealed class CrossScopeTransferException : DomainException
{
    public CrossScopeTransferException()
        : base("Nao e possivel transferir entre contas de OwnerScopeId (usuarios) diferentes.")
    {
    }
}
