using FinanceControl.Application.DTOs;

namespace FinanceControl.Application.Validators;

public sealed class CreateCategoryRequestValidator : IValidator<CreateCategoryRequest>
{
    public ValidationResult Validate(CreateCategoryRequest request)
    {
        var errors = new List<ValidationError>();

        if (request.OwnerScopeId == Guid.Empty)
            errors.Add(new ValidationError(nameof(request.OwnerScopeId), "OwnerScopeId e obrigatorio."));

        if (string.IsNullOrWhiteSpace(request.Name))
            errors.Add(new ValidationError(nameof(request.Name), "O nome da categoria e obrigatorio."));

        return new ValidationResult(errors);
    }
}

public sealed class UpdateCategoryRequestValidator : IValidator<UpdateCategoryRequest>
{
    public ValidationResult Validate(UpdateCategoryRequest request)
    {
        var errors = new List<ValidationError>();

        if (request.OwnerScopeId == Guid.Empty)
            errors.Add(new ValidationError(nameof(request.OwnerScopeId), "OwnerScopeId e obrigatorio."));

        if (request.CategoryId == Guid.Empty)
            errors.Add(new ValidationError(nameof(request.CategoryId), "CategoryId e obrigatorio."));

        if (string.IsNullOrWhiteSpace(request.Name))
            errors.Add(new ValidationError(nameof(request.Name), "O nome da categoria e obrigatorio."));

        return new ValidationResult(errors);
    }
}
