using FinanceControl.Application.DTOs;

namespace FinanceControl.Application.Validators;

public sealed class CreateTransferRequestValidator : IValidator<CreateTransferRequest>
{
    public ValidationResult Validate(CreateTransferRequest request)
    {
        var errors = new List<ValidationError>();

        if (request.OwnerScopeId == Guid.Empty)
            errors.Add(new ValidationError(nameof(request.OwnerScopeId), "OwnerScopeId e obrigatorio."));

        if (request.SourceAccountId == Guid.Empty)
            errors.Add(new ValidationError(nameof(request.SourceAccountId), "SourceAccountId e obrigatorio."));

        if (request.DestinationAccountId == Guid.Empty)
            errors.Add(new ValidationError(nameof(request.DestinationAccountId), "DestinationAccountId e obrigatorio."));

        // Checagem rapida (fail-fast) antes de tocar em repositorio/Domain.
        // TransferGroup.Create no Domain tambem valida isso (fonte da
        // verdade) - aqui e so para devolver um erro de validacao mais
        // barato quando o problema e obvio na propria requisicao.
        if (request.SourceAccountId != Guid.Empty &&
            request.DestinationAccountId != Guid.Empty &&
            request.SourceAccountId == request.DestinationAccountId)
        {
            errors.Add(new ValidationError(nameof(request.DestinationAccountId), "Nao e possivel transferir de uma conta para ela mesma."));
        }

        if (request.Amount <= 0)
            errors.Add(new ValidationError(nameof(request.Amount), "O valor deve ser maior que zero."));

        if (request.TransferDate == default)
            errors.Add(new ValidationError(nameof(request.TransferDate), "A data da transferencia e obrigatoria."));

        return new ValidationResult(errors);
    }
}
