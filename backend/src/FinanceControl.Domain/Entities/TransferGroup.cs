using FinanceControl.Domain.Exceptions;

namespace FinanceControl.Domain.Entities;

/// <summary>
/// Representa uma transferencia entre duas contas (regra 6/7 do MVP).
/// TransferGroup.Create e o UNICO ponto de entrada para criar uma
/// transferencia - garante atomicamente as duas Transactions (TransferOut
/// na origem, TransferIn no destino), mesmo valor, e nunca contabilizadas
/// como receita/despesa.
/// </summary>
public sealed class TransferGroup
{
    public Guid Id { get; }
    public Guid OwnerScopeId { get; }
    public Guid SourceAccountId { get; }
    public Guid DestinationAccountId { get; }
    public decimal Amount { get; }
    public DateOnly TransferDate { get; }
    public string? Description { get; }
    public DateTime CreatedAt { get; }

    private TransferGroup(
        Guid id,
        Guid ownerScopeId,
        Guid sourceAccountId,
        Guid destinationAccountId,
        decimal amount,
        DateOnly transferDate,
        string? description,
        DateTime createdAt)
    {
        Id = id;
        OwnerScopeId = ownerScopeId;
        SourceAccountId = sourceAccountId;
        DestinationAccountId = destinationAccountId;
        Amount = amount;
        TransferDate = transferDate;
        Description = description;
        CreatedAt = createdAt;
    }

    public static TransferGroup Create(
        Account source,
        Account destination,
        decimal amount,
        DateOnly transferDate,
        string? description = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        if (source.Id == destination.Id)
            throw new TransferToSameAccountException();

        if (source.OwnerScopeId != destination.OwnerScopeId)
            throw new CrossScopeTransferException();

        if (source.Type == FinanceControl.Domain.Enums.AccountType.CreditCard || destination.Type == FinanceControl.Domain.Enums.AccountType.CreditCard)
            throw new InvalidOperationException("Transferencias entre contas de credito nao fazem parte do MVP; use apenas contas financeiras.");

        if (amount <= 0)
            throw new InvalidMonetaryAmountException(amount);

        var group = new TransferGroup(
            Guid.NewGuid(), source.OwnerScopeId, source.Id, destination.Id,
            amount, transferDate, description, DateTime.UtcNow);

        // As duas pernas sao criadas juntas, dentro do mesmo metodo -
        // nao ha como uma existir sem a outra.
        source.RegisterTransferOut(amount, group.Id, transferDate, description);
        destination.RegisterTransferIn(amount, group.Id, transferDate, description);

        return group;
    }
}
