using FinanceControl.Application.DTOs;

namespace FinanceControl.Application.Validators;

public sealed class RegisterIncomeRequestValidator : IValidator<RegisterIncomeRequest>
{
    public ValidationResult Validate(RegisterIncomeRequest request)
    {
        var errors = new List<ValidationError>();

        if (request.OwnerScopeId == Guid.Empty)
            errors.Add(new ValidationError(nameof(request.OwnerScopeId), "OwnerScopeId e obrigatorio."));

        if (request.AccountId == Guid.Empty)
            errors.Add(new ValidationError(nameof(request.AccountId), "AccountId e obrigatorio."));

        if (request.Amount <= 0)
            errors.Add(new ValidationError(nameof(request.Amount), "O valor deve ser maior que zero."));

        if (request.CompetenceDate == default)
            errors.Add(new ValidationError(nameof(request.CompetenceDate), "A data de competencia e obrigatoria."));

        if (string.IsNullOrWhiteSpace(request.Description))
            errors.Add(new ValidationError(nameof(request.Description), "A descricao e obrigatoria."));

        if (request.CategoryId is Guid categoryId && categoryId == Guid.Empty)
            errors.Add(new ValidationError(nameof(request.CategoryId), "CategoryId informado e invalido."));

        return new ValidationResult(errors);
    }
}

public sealed class RegisterExpenseRequestValidator : IValidator<RegisterExpenseRequest>
{
    public ValidationResult Validate(RegisterExpenseRequest request)
    {
        var errors = new List<ValidationError>();

        if (request.OwnerScopeId == Guid.Empty)
            errors.Add(new ValidationError(nameof(request.OwnerScopeId), "OwnerScopeId e obrigatorio."));

        if (request.AccountId == Guid.Empty)
            errors.Add(new ValidationError(nameof(request.AccountId), "AccountId e obrigatorio."));

        if (request.Amount <= 0)
            errors.Add(new ValidationError(nameof(request.Amount), "O valor deve ser maior que zero."));

        if (request.CompetenceDate == default)
            errors.Add(new ValidationError(nameof(request.CompetenceDate), "A data de competencia e obrigatoria."));

        if (string.IsNullOrWhiteSpace(request.Description))
            errors.Add(new ValidationError(nameof(request.Description), "A descricao e obrigatoria."));

        if (request.CategoryId is Guid categoryId && categoryId == Guid.Empty)
            errors.Add(new ValidationError(nameof(request.CategoryId), "CategoryId informado e invalido."));

        return new ValidationResult(errors);
    }
}
