using FinanceControl.Domain.Exceptions;
using FinanceControl.Domain.ValueObjects;

namespace FinanceControl.Domain.Tests;

public sealed class CreditCardSettingsTests
{
    [Fact]
    public void Create_RejectsInvalidDays()
    {
        Assert.Throws<InvalidCreditCardSettingsException>(() =>
            CreditCardSettings.Create(3000m, 0, 20));

        Assert.Throws<InvalidCreditCardSettingsException>(() =>
            CreditCardSettings.Create(3000m, 10, 32));
    }

    [Fact]
    public void EqualSettings_AreEqual()
    {
        var first = CreditCardSettings.Create(3000m, 10, 20);
        var second = CreditCardSettings.Create(3000m, 10, 20);

        Assert.Equal(first, second);
    }
}
