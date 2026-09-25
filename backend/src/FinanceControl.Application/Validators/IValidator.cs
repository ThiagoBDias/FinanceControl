namespace FinanceControl.Application.Validators;

public interface IValidator<in TRequest>
{
    ValidationResult Validate(TRequest request);
}
