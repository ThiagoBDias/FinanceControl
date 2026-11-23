using FinanceControl.Domain.Entities;
using FinanceControl.Domain.Enums;
using FinanceControl.Domain.Exceptions;

namespace FinanceControl.Domain.Tests;

public sealed partial class TransferGroupTests
{
    [Fact]
    public void CreateTransfer_ChangesBothBalancesWithoutCreatingIncomeOrExpense()
    {
        var owner = Guid.NewGuid();
        var source = Account.CreateAsset(owner, "Corrente", AccountType.Checking, 1000m);
        var destination = Account.CreateAsset(owner, "Poupanca", AccountType.Savings, 200m);

        var transfer = TransferGroup.Create(source, destination, 300m, new DateOnly(2026, 9, 24), "Reserva");

        Assert.Equal(300m, transfer.Amount);
        Assert.Equal(700m, source.CalculatedBalance);
        Assert.Equal(500m, destination.CalculatedBalance);
        Assert.Single(source.Transactions);
        Assert.Single(destination.Transactions);
        Assert.Equal(TransactionType.TransferOut, source.Transactions.Single().Type);
        Assert.Equal(TransactionType.TransferIn, destination.Transactions.Single().Type);
    }

    [Fact]
    public void CreateTransfer_ToSameAccount_Throws()
    {
        var owner = Guid.NewGuid();
        var account = Account.CreateAsset(owner, "Conta", AccountType.Checking, 1000m);

        Assert.Throws<TransferToSameAccountException>(() =>
            TransferGroup.Create(account, account, 100m, new DateOnly(2026, 9, 24)));
    }

    [Fact]
    public void CreateTransfer_AcrossOwners_Throws()
    {
        var source = Account.CreateAsset(Guid.NewGuid(), "A", AccountType.Checking, 1000m);
        var destination = Account.CreateAsset(Guid.NewGuid(), "B", AccountType.Savings, 0m);

        Assert.Throws<CrossScopeTransferException>(() =>
            TransferGroup.Create(source, destination, 100m, new DateOnly(2026, 9, 24)));
    }

    [Fact]
    public void CreateTransfer_WithNonPositiveAmount_Throws()
    {
        var owner = Guid.NewGuid();
        var source = Account.CreateAsset(owner, "A", AccountType.Checking, 1000m);
        var destination = Account.CreateAsset(owner, "B", AccountType.Savings, 0m);

        Assert.Throws<InvalidMonetaryAmountException>(() =>
            TransferGroup.Create(source, destination, 0m, new DateOnly(2026, 9, 24)));
    }
}


public partial class TransferGroupTests
{
    [Fact]
    public void Create_ShouldRejectCreditCardAsTransferEndpoint()
    {
        var owner = Guid.NewGuid();
        var card = Account.CreateCreditCard(owner, "Cartao", FinanceControl.Domain.ValueObjects.CreditCardSettings.Create(3000m, 10, 20));
        var checking = Account.CreateAsset(owner, "Conta", AccountType.Checking, 1000m);

        Assert.Throws<OperationRejectedException>(() =>
            TransferGroup.Create(checking, card, 100m, new DateOnly(2026, 9, 24)));
    }
}
