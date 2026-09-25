using FinanceControl.Domain.Enums;

namespace FinanceControl.Domain.Entities;

public sealed class Category
{
    public Guid Id { get; }
    public Guid OwnerScopeId { get; }
    public string Name { get; private set; }
    public CategoryType Type { get; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; private set; }

    private Category(Guid id, Guid ownerScopeId, string name, CategoryType type, DateTime createdAt)
    {
        Id = id;
        OwnerScopeId = ownerScopeId;
        Name = name;
        Type = type;
        IsActive = true;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public static Category Create(Guid ownerScopeId, string name, CategoryType type)
    {
        return new Category(Guid.NewGuid(), ownerScopeId, NormalizeName(name), type, DateTime.UtcNow);
    }

    /// <summary>Adicionado para suportar o caso de uso UpdateCategory da Application layer.</summary>
    public void Rename(string name)
    {
        Name = NormalizeName(name);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Adicionado para suportar o caso de uso DeactivateCategory da Application layer.</summary>
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

    private static string NormalizeName(string name)
    {
        var normalized = (name ?? string.Empty).Trim();
        if (normalized.Length == 0)
            throw new ArgumentException("O nome da categoria e obrigatorio.", nameof(name));

        return normalized;
    }
}
