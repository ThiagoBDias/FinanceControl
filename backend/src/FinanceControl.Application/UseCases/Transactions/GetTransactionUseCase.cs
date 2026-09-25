using FinanceControl.Application.DTOs;
using FinanceControl.Application.Interfaces;
using FinanceControl.Application.Validators;
using FinanceControl.Domain.Entities;

namespace FinanceControl.Application.UseCases.Transactions;

public sealed class GetTransactionUseCase : IUseCase<GetTransactionRequest, TransactionResponse>
{
    private readonly ITransactionRepository _transactionRepository;

    public GetTransactionUseCase(ITransactionRepository transactionRepository) => _transactionRepository = transactionRepository;

    public async Task<TransactionResponse> ExecuteAsync(GetTransactionRequest request, CancellationToken cancellationToken = default)
    {
        var transaction = await _transactionRepository.GetByIdAsync(request.TransactionId, request.OwnerScopeId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Transaction), request.TransactionId);

        return TransactionMapper.ToResponse(transaction);
    }
}
