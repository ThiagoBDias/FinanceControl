using FinanceControl.Domain.Enums;

namespace FinanceControl.Domain.Exceptions;

public sealed class TransferGroupRequiredException : DomainException
{
    public TransferGroupRequiredException()
        : base("Transacoes do tipo TransferIn/TransferOut precisam pertencer a um TransferGroup.")
    {
    }
}

public sealed class TransferGroupNotAllowedException : DomainException
{
    public TransferGroupNotAllowedException()
        : base("Apenas transacoes do tipo TransferIn/TransferOut podem pertencer a um TransferGroup.")
    {
    }
}

public sealed class CategoryNotAllowedForTransferException : DomainException
{
    public CategoryNotAllowedForTransferException()
        : base("Transferencias nao podem possuir categoria.")
    {
    }
}

public sealed class CategoryTypeMismatchException : DomainException
{
    public CategoryTypeMismatchException(CategoryType categoryType, TransactionType transactionType)
        : base($"A categoria do tipo '{categoryType}' nao pode ser usada em uma transacao do tipo '{transactionType}'.")
    {
    }
}

public sealed class InactiveCategoryException : DomainException
{
    public InactiveCategoryException(Guid categoryId)
        : base($"A categoria '{categoryId}' esta inativa e nao pode ser usada em novos lancamentos.")
    {
    }
}
