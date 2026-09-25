using FinanceControl.Domain.Enums;
using FinanceControl.Domain.Exceptions;
using FinanceControl.Domain.ValueObjects;

namespace FinanceControl.Domain.Entities;

/// <summary>
/// Conta financeira. E o aggregate root que controla a criacao de
/// Transactions (garantindo os invariantes de valor/categoria/fatura) e
/// calcula o proprio saldo.
///
/// Regra de ouro do ledger: o efeito de cada TransactionType sobre o saldo
/// depende da natureza contabil da conta (Asset ou Liability). Contas
/// Checking/Savings/Cash/Investment sao Asset; CreditCard e Liability
/// (o "saldo" representa divida). Ver SignedEffect.
/// </summary>
public sealed class Account
{
    private readonly List<Transaction> _transactions = new();

    public Guid Id { get; }
    public Guid OwnerScopeId { get; }
    public string Name { get; private set; }
    public AccountType Type { get; }
    public decimal InitialBalance { get; }
    public decimal CachedBalance { get; private set; }
    public DateTime CachedBalanceUpdatedAt { get; private set; }
    public CreditCardSettings? CreditCardSettings { get; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

    /// <summary>
    /// Saldo real, sempre recalculado a partir do ledger (InitialBalance +
    /// efeito de todas as transacoes ativas). E a fonte da verdade;
    /// CachedBalance existe apenas por performance (ver AttachTransaction).
    /// </summary>
    public decimal CalculatedBalance =>
        InitialBalance + _transactions.Where(t => !t.IsDeleted).Sum(SignedEffect);

    /// <summary>
    /// Natureza contabil da conta. Somente CreditCard e Liability nesta
    /// etapa do MVP.
    /// </summary>
    private bool IsLiability => Type == AccountType.CreditCard;

    private Account(
        Guid id,
        Guid ownerScopeId,
        string name,
        AccountType type,
        decimal initialBalance,
        CreditCardSettings? creditCardSettings,
        DateTime createdAt)
    {
        Id = id;
        OwnerScopeId = ownerScopeId;
        Name = name;
        Type = type;
        InitialBalance = initialBalance;
        CachedBalance = initialBalance;
        CachedBalanceUpdatedAt = createdAt;
        CreditCardSettings = creditCardSettings;
        IsActive = true;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    /// <summary>Cria uma conta Checking, Savings, Cash ou Investment.</summary>
    public static Account CreateAsset(Guid ownerScopeId, string name, AccountType type, decimal initialBalance)
    {
        if (type == AccountType.CreditCard)
            throw new InvalidCreditCardSettingsException(
                "Contas do tipo CreditCard devem ser criadas com Account.CreateCreditCard, informando o CreditCardSettings.");

        var normalizedName = NormalizeName(name);

        if (initialBalance < 0)
            throw new InvalidMonetaryAmountException(initialBalance);

        return new Account(Guid.NewGuid(), ownerScopeId, normalizedName, type, initialBalance, creditCardSettings: null, DateTime.UtcNow);
    }

    /// <summary>Cria uma conta do tipo CreditCard (Liability), com suas configuracoes especificas.</summary>
    public static Account CreateCreditCard(Guid ownerScopeId, string name, CreditCardSettings settings, decimal initialBalance = 0)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var normalizedName = NormalizeName(name);

        if (initialBalance < 0)
            throw new InvalidMonetaryAmountException(initialBalance);

        return new Account(Guid.NewGuid(), ownerScopeId, normalizedName, AccountType.CreditCard, initialBalance, settings, DateTime.UtcNow);
    }

    /// <summary>
    /// Adicionado para suportar o caso de uso UpdateAccount da Application
    /// layer. Unica propriedade editavel de Account alem de IsActive -
    /// Type, InitialBalance e CreditCardSettings permanecem imutaveis
    /// apos a criacao.
    /// </summary>
    public void Rename(string name)
    {
        Name = NormalizeName(name);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Registra uma despesa (regra 8: compra no cartao e Expense, aumenta a
    /// obrigacao e pertence a fatura correspondente).
    /// </summary>
    public Transaction RegisterExpense(
        decimal amount,
        DateOnly competenceDate,
        string description,
        Category? category = null,
        DateOnly? dueDate = null,
        Guid? invoiceId = null)
    {
        EnsureActive();

        if (category is not null)
            EnsureCategoryMatchesType(category, TransactionType.Expense);

        if (Type == AccountType.CreditCard && invoiceId is null)
            throw new InvoiceRequiredForCreditCardExpenseException(Id);

        if (Type != AccountType.CreditCard && invoiceId is not null)
            throw new InvoiceNotAllowedForNonCreditCardAccountException(Id);

        var transaction = Transaction.CreateSimple(
            OwnerScopeId, Id, category?.Id, invoiceId, TransactionType.Expense, amount, description, competenceDate, dueDate);

        AttachTransaction(transaction);
        return transaction;
    }

    public Transaction RegisterIncome(
        decimal amount,
        DateOnly competenceDate,
        string description,
        Category? category = null,
        DateOnly? dueDate = null)
    {
        EnsureActive();

        if (category is not null)
            EnsureCategoryMatchesType(category, TransactionType.Income);

        var transaction = Transaction.CreateSimple(
            OwnerScopeId, Id, category?.Id, invoiceId: null, TransactionType.Income, amount, description, competenceDate, dueDate);

        AttachTransaction(transaction);
        return transaction;
    }

    /// <summary>
    /// Cria a perna de saida de uma transferencia. So deve ser chamado por
    /// TransferGroup.Create - garante que toda transferencia tenha as duas
    /// pernas criadas atomicamente (regra 6/7).
    /// </summary>
    internal Transaction RegisterTransferOut(
        decimal amount,
        Guid transferGroupId,
        DateOnly competenceDate,
        string? description)
    {
        EnsureActive();

        var transaction = Transaction.CreateTransferLeg(
            OwnerScopeId, Id, TransactionType.TransferOut, amount, description, competenceDate, transferGroupId);

        AttachTransaction(transaction);
        return transaction;
    }

    /// <summary>Cria a perna de entrada de uma transferencia. Ver RegisterTransferOut.</summary>
    internal Transaction RegisterTransferIn(
        decimal amount,
        Guid transferGroupId,
        DateOnly competenceDate,
        string? description)
    {
        EnsureActive();

        var transaction = Transaction.CreateTransferLeg(
            OwnerScopeId, Id, TransactionType.TransferIn, amount, description, competenceDate, transferGroupId);

        AttachTransaction(transaction);
        return transaction;
    }

    private void AttachTransaction(Transaction transaction)
    {
        _transactions.Add(transaction);

        // CachedBalance e mantido incrementalmente (mesma formula de
        // CalculatedBalance) apenas por performance - CalculatedBalance
        // continua sendo a fonte da verdade, recalculada do zero.
        CachedBalance += SignedEffect(transaction);
        CachedBalanceUpdatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    private void EnsureActive()
    {
        if (!IsActive)
            throw new InactiveAccountException(Id);
    }

    private static void EnsureCategoryMatchesType(Category category, TransactionType transactionType)
    {
        if (!category.IsActive)
            throw new InactiveCategoryException(category.Id);

        var expected = transactionType == TransactionType.Income ? CategoryType.Income : CategoryType.Expense;

        if (category.Type != expected)
            throw new CategoryTypeMismatchException(category.Type, transactionType);
    }

    private static string NormalizeName(string name)
    {
        var normalized = (name ?? string.Empty).Trim();
        if (normalized.Length == 0)
            throw new ArgumentException("O nome da conta e obrigatorio.", nameof(name));

        return normalized;
    }

    /// <summary>
    /// Tabela de sinais definitiva do ledger (rodada 2 da revisao
    /// arquitetural): Income/Expense/TransferIn/TransferOut tem efeito
    /// oposto em contas Asset vs. Liability.
    /// </summary>
    private decimal SignedEffect(Transaction transaction) => (transaction.Type, IsLiability) switch
    {
        (TransactionType.Income, false) => transaction.Amount,
        (TransactionType.Expense, false) => -transaction.Amount,
        (TransactionType.TransferIn, false) => transaction.Amount,
        (TransactionType.TransferOut, false) => -transaction.Amount,

        (TransactionType.Income, true) => -transaction.Amount,
        (TransactionType.Expense, true) => transaction.Amount,
        (TransactionType.TransferIn, true) => -transaction.Amount,
        (TransactionType.TransferOut, true) => transaction.Amount,

        _ => throw new InvalidOperationException($"Tipo de transacao nao suportado: {transaction.Type}.")
    };
}
