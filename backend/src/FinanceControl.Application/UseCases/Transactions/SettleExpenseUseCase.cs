using FinanceControl.Application.DTOs;
using FinanceControl.Application.Interfaces;
using FinanceControl.Application.Validators;
using FinanceControl.Domain.Entities;

namespace FinanceControl.Application.UseCases.Transactions;

public sealed class SettleExpenseUseCase(ITransactionRepository transactions, IAccountRepository accounts, IUnitOfWork unitOfWork)
{
    public async Task<TransactionResponse> ExecuteAsync(Guid ownerId, Guid transactionId, DateOnly paymentDate, CancellationToken cancellationToken)
    {
        FinancialDates.ValidatePaymentDate(paymentDate);
        var transaction = await transactions.GetByIdAsync(transactionId, ownerId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Transaction), transactionId);
        var account = await accounts.GetByIdAsync(transaction.AccountId, ownerId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Account), transaction.AccountId);
        account.SettleExpense(transaction, paymentDate);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return TransactionMapper.ToResponse(transaction);
    }
}
