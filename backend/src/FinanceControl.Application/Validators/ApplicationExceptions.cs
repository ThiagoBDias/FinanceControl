namespace FinanceControl.Application.Validators;

/// <summary>
/// A estrutura pedida para a Application (UseCases/DTOs/Interfaces/
/// Validators) nao previa uma pasta Exceptions dedicada. Como estas duas
/// excecoes existem apenas para apoiar o fluxo de validacao/orquestracao
/// dos casos de uso (e nao sao regra de negocio do Domain), ficaram aqui,
/// junto da infraestrutura de validacao que as origina.
/// </summary>
public sealed class RequestValidationException : Exception
{
    public IReadOnlyList<ValidationError> Errors { get; }

    public RequestValidationException(IReadOnlyList<ValidationError> errors)
        : base("Um ou mais campos da requisicao sao invalidos: " +
               string.Join("; ", errors.Select(e => $"{e.PropertyName}: {e.Message}")))
    {
        Errors = errors;
    }
}

/// <summary>
/// Lancada quando um recurso nao e encontrado - inclusive quando ele
/// pertence a outro OwnerScopeId, ja que os repositorios sempre buscam
/// filtrando por ownerScopeId (isolamento multiusuario, regra 13/14 da
/// arquitetura). Do ponto de vista de quem chama, "existe mas nao e seu"
/// e indistinguivel de "nao existe" - e assim que o isolamento deve se
/// comportar.
/// </summary>
public sealed class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string entityName, Guid id)
        : base($"{entityName} '{id}' nao foi encontrado(a) para este usuario.")
    {
    }
}
