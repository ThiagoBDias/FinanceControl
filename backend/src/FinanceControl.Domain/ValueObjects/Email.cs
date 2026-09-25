using System.Text.RegularExpressions;
using FinanceControl.Domain.Exceptions;

namespace FinanceControl.Domain.ValueObjects;

/// <summary>
/// Value Object imutavel. Garante que nenhum User exista com e-mail
/// em formato invalido.
/// </summary>
public sealed class Email : IEquatable<Email>
{
    private static readonly Regex Pattern = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string value)
    {
        var normalized = (value ?? string.Empty).Trim();

        if (normalized.Length == 0 || !Pattern.IsMatch(normalized))
            throw new InvalidEmailException(value ?? string.Empty);

        return new Email(normalized.ToLowerInvariant());
    }

    public override string ToString() => Value;

    public bool Equals(Email? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => Equals(obj as Email);
    public override int GetHashCode() => Value.GetHashCode();
}
