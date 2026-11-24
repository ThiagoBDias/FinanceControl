namespace FinanceControl.Domain.ValueObjects;

public static class Money
{
    // Numeric(18,2) is the storage contract for individual ledger values.
    public const decimal Maximum = 9_999_999_999_999_999.99m;

    public static bool IsValid(decimal value) =>
        value >= 0 && value <= Maximum && decimal.Round(value, 2) == value;

    public static decimal Installment(decimal total, int count, int number)
    {
        var baseAmount = decimal.Truncate(total * 100 / count) / 100;
        var remainingCents = (total - baseAmount * count) * 100;
        return baseAmount + (number <= remainingCents ? 0.01m : 0m);
    }
}
