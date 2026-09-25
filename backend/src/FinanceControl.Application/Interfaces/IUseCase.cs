namespace FinanceControl.Application.Interfaces;

/// <summary>
/// Contrato uniforme para um caso de uso: recebe um request, devolve um
/// response. Nao e MediatR/CQRS - so um formato consistente, chamado
/// diretamente via injecao de dependencia (sem dispatcher/mediator).
/// </summary>
public interface IUseCase<in TRequest, TResponse>
{
    Task<TResponse> ExecuteAsync(TRequest request, CancellationToken cancellationToken = default);
}
