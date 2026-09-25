using FinanceControl.Domain.ValueObjects;

namespace FinanceControl.Domain.Entities;

/// <summary>
/// Usuario do sistema, na perspectiva do Domain financeiro (dono dos dados).
/// Credenciais de autenticacao (senha, tokens) NAO vivem aqui - pertencem
/// ao ApplicationUser do ASP.NET Core Identity, na Infrastructure, fora do
/// escopo desta etapa.
/// </summary>
public sealed class User
{
    public Guid Id { get; }
    public string Name { get; }
    public Email Email { get; }
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; }

    private User(Guid id, string name, Email email, DateTime createdAt)
    {
        Id = id;
        Name = name;
        Email = email;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public static User Create(string name, string email) => Create(Guid.NewGuid(), name, email);

    public static User Create(Guid id, string name, string email)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("O id do usuario e obrigatorio.", nameof(id));

        var normalizedName = (name ?? string.Empty).Trim();
        if (normalizedName.Length == 0)
            throw new ArgumentException("O nome do usuario e obrigatorio.", nameof(name));

        return new User(id, normalizedName, Email.Create(email), DateTime.UtcNow);
    }
}
