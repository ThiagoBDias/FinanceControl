using FinanceControl.Application.DTOs;
using FinanceControl.Application.Interfaces;

namespace FinanceControl.Application.UseCases.Transactions;

public sealed class ListTransactionsUseCase : IUseCase<ListTransactionsRequest, IReadOnlyList<TransactionResponse>>
{
    private readonly ITransactionRepository _transactionRepository;

    public ListTransactionsUseCase(ITransactionRepository transactionRepository) => _transactionRepository = transactionRepository;

    public async Task<IReadOnlyList<TransactionResponse>> ExecuteAsync(ListTransactionsRequest request, CancellationToken cancellationToken = default)
    {
        var transactions = await _transactionRepository.ListAsync(request, cancellationToken);
        return transactions.Select(TransactionMapper.ToResponse).ToList();
    }
}
