using FinanceControl.Application.DTOs;
using FinanceControl.Domain.Enums;

namespace FinanceControl.Application.Validators;

public sealed class CreateAccountRequestValidator : IValidator<CreateAccountRequest>
{
    public ValidationResult Validate(CreateAccountRequest request)
    {
        var errors = new List<ValidationError>();

        if (request.OwnerScopeId == Guid.Empty)
            errors.Add(new ValidationError(nameof(request.OwnerScopeId), "OwnerScopeId e obrigatorio."));

        if (string.IsNullOrWhiteSpace(request.Name))
            errors.Add(new ValidationError(nameof(request.Name), "O nome da conta e obrigatorio."));

        if (request.InitialBalance < 0)
            errors.Add(new ValidationError(nameof(request.InitialBalance), "O saldo inicial nao pode ser negativo."));

        if (request.Type == AccountType.CreditCard)
            errors.Add(new ValidationError(nameof(request.Type),
                "Contas do tipo CreditCard devem ser criadas atraves do caso de uso CreateCreditCard."));

        return new ValidationResult(errors);
    }
}

public sealed class UpdateAccountRequestValidator : IValidator<UpdateAccountRequest>
{
    public ValidationResult Validate(UpdateAccountRequest request)
    {
        var errors = new List<ValidationError>();

        if (request.OwnerScopeId == Guid.Empty)
            errors.Add(new ValidationError(nameof(request.OwnerScopeId), "OwnerScopeId e obrigatorio."));

        if (request.AccountId == Guid.Empty)
            errors.Add(new ValidationError(nameof(request.AccountId), "AccountId e obrigatorio."));

        if (string.IsNullOrWhiteSpace(request.Name))
            errors.Add(new ValidationError(nameof(request.Name), "O nome da conta e obrigatorio."));

        return new ValidationResult(errors);
    }
}
