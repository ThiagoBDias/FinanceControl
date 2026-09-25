using FinanceControl.Application.DTOs;
using FinanceControl.Application.Interfaces;
using FinanceControl.Application.Validators;
using FinanceControl.Domain.Entities;

namespace FinanceControl.Application.UseCases.Transactions;

public sealed class DeleteTransactionUseCase : IUseCase<GetTransactionRequest, TransactionResponse>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTransactionUseCase(ITransactionRepository transactionRepository, IUnitOfWork unitOfWork)
    {
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TransactionResponse> ExecuteAsync(GetTransactionRequest request, CancellationToken cancellationToken = default)
    {
        if (request.OwnerScopeId == Guid.Empty || request.TransactionId == Guid.Empty)
            throw new RequestValidationException(new[] { new ValidationError(nameof(request.TransactionId), "OwnerScopeId e TransactionId sao obrigatorios.") });

        var transaction = await _transactionRepository.GetByIdAsync(request.TransactionId, request.OwnerScopeId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Transaction), request.TransactionId);

        if (transaction.TransferGroupId is not null)
            throw new RequestValidationException(new[] { new ValidationError(nameof(request.TransactionId), "Lancamentos de transferencia devem ser tratados como um grupo e nao podem ser excluidos individualmente.") });

        transaction.Delete();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return TransactionMapper.ToResponse(transaction);
    }
}
