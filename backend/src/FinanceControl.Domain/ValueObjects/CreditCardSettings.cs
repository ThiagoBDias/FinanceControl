using FinanceControl.Domain.Exceptions;

namespace FinanceControl.Domain.ValueObjects;

/// <summary>
/// Value Object imutavel com as configuracoes especificas de uma conta
/// do tipo CreditCard (regra 5 do MVP). So existe atrelado a um Account
/// com AccountType.CreditCard.
/// </summary>
public sealed class CreditCardSettings : IEquatable<CreditCardSettings>
{
    public decimal CreditLimit { get; }
    public int ClosingDay { get; }
    public int DueDay { get; }

    private CreditCardSettings(decimal creditLimit, int closingDay, int dueDay)
    {
        CreditLimit = creditLimit;
        ClosingDay = closingDay;
        DueDay = dueDay;
    }

    public static CreditCardSettings Create(decimal creditLimit, int closingDay, int dueDay)
    {
        if (!Money.IsValid(creditLimit))
            throw new InvalidCreditCardSettingsException("O limite do cartao nao pode ser negativo.");

        if (closingDay is < 1 or > 31)
            throw new InvalidCreditCardSettingsException("O dia de fechamento deve estar entre 1 e 31.");

        if (dueDay is < 1 or > 31)
            throw new InvalidCreditCardSettingsException("O dia de vencimento deve estar entre 1 e 31.");

        return new CreditCardSettings(creditLimit, closingDay, dueDay);
    }

    public bool Equals(CreditCardSettings? other) =>
        other is not null &&
        CreditLimit == other.CreditLimit &&
        ClosingDay == other.ClosingDay &&
        DueDay == other.DueDay;

    public override bool Equals(object? obj) => Equals(obj as CreditCardSettings);
    public override int GetHashCode() => HashCode.Combine(CreditLimit, ClosingDay, DueDay);
}
