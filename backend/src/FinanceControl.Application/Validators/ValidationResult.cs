namespace FinanceControl.Application.Validators;

public sealed record ValidationError(string PropertyName, string Message);

public sealed class ValidationResult
{
    public IReadOnlyList<ValidationError> Errors { get; }

    public ValidationResult(IEnumerable<ValidationError> errors) => Errors = errors.ToList();

    public bool IsValid => Errors.Count == 0;

    public static ValidationResult Success() => new(Array.Empty<ValidationError>());
}
