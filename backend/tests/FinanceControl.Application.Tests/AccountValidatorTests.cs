using FinanceControl.Application.DTOs;
using FinanceControl.Application.Validators;
using FinanceControl.Domain.Enums;

namespace FinanceControl.Application.Tests;

public sealed class AccountValidatorTests
{
    [Fact]
    public void CreateAccount_RejectsCreditCardType()
    {
        var validator = new CreateAccountRequestValidator();
        var request = new CreateAccountRequest(Guid.NewGuid(), "Cartao", AccountType.CreditCard, 0m);

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.Type));
    }

    [Fact]
    public void CreateAccount_RejectsMissingNameAndNegativeBalance()
    {
        var validator = new CreateAccountRequestValidator();
        var request = new CreateAccountRequest(Guid.NewGuid(), "", AccountType.Checking, -10m);

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.Name));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.InitialBalance));
    }
}
