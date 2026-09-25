using FinanceControl.Application.DTOs;

namespace FinanceControl.Application.Validators;

public sealed class CreateCreditCardRequestValidator : IValidator<CreateCreditCardRequest>
{
    public ValidationResult Validate(CreateCreditCardRequest request)
    {
        var errors = new List<ValidationError>();

        if (request.OwnerScopeId == Guid.Empty)
            errors.Add(new ValidationError(nameof(request.OwnerScopeId), "OwnerScopeId e obrigatorio."));

        if (string.IsNullOrWhiteSpace(request.Name))
            errors.Add(new ValidationError(nameof(request.Name), "O nome do cartao e obrigatorio."));

        if (request.CreditLimit < 0)
            errors.Add(new ValidationError(nameof(request.CreditLimit), "O limite do cartao nao pode ser negativo."));

        if (request.ClosingDay is < 1 or > 31)
            errors.Add(new ValidationError(nameof(request.ClosingDay), "O dia de fechamento deve estar entre 1 e 31."));

        if (request.DueDay is < 1 or > 31)
            errors.Add(new ValidationError(nameof(request.DueDay), "O dia de vencimento deve estar entre 1 e 31."));

        if (request.InitialBalance < 0)
            errors.Add(new ValidationError(nameof(request.InitialBalance), "A divida inicial nao pode ser negativa."));

        return new ValidationResult(errors);
    }
}
