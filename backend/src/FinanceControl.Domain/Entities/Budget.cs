namespace FinanceControl.Domain.Entities;

/// <summary>
/// Limite mensal de gastos para uma categoria.
/// O valor gasto nao e armazenado: e calculado a partir dos lancamentos.
/// </summary>
public sealed class Budget
{
    public Guid Id { get; }
    public Guid OwnerScopeId { get; }
    public Guid CategoryId { get; }
    public int Year { get; }
    public int Month { get; }
    public decimal LimitAmount { get; private set; }
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; private set; }

    private Budget(Guid id, Guid ownerScopeId, Guid categoryId, int year, int month, decimal limitAmount, DateTime createdAt)
    {
        Validate(year, month, limitAmount);
        Id = id;
        OwnerScopeId = ownerScopeId;
        CategoryId = categoryId;
        Year = year;
        Month = month;
        LimitAmount = limitAmount;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public static Budget Create(Guid ownerScopeId, Guid categoryId, int year, int month, decimal limitAmount)
        => new(Guid.NewGuid(), ownerScopeId, categoryId, year, month, limitAmount, DateTime.UtcNow);

    public void ChangeLimit(decimal limitAmount)
    {
        if (limitAmount <= 0)
            throw new ArgumentOutOfRangeException(nameof(limitAmount), "O limite deve ser maior que zero.");

        LimitAmount = limitAmount;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void Validate(int year, int month, decimal limitAmount)
    {
        if (year < 2000 || year > 2100)
            throw new ArgumentOutOfRangeException(nameof(year), "Ano invalido.");
        if (month is < 1 or > 12)
            throw new ArgumentOutOfRangeException(nameof(month), "Mes invalido.");
        if (limitAmount <= 0)
            throw new ArgumentOutOfRangeException(nameof(limitAmount), "O limite deve ser maior que zero.");
    }
}
