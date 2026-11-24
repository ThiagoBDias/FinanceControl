using FinanceControl.Domain.Exceptions;
using FinanceControl.Application.DTOs;
using FinanceControl.Application.Interfaces;
using FinanceControl.Application.Validators;
using FinanceControl.Domain.Entities;

namespace FinanceControl.Application.UseCases.Transactions;

public sealed class DeleteTransactionUseCase : IUseCase<GetTransactionRequest, TransactionResponse>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccountRepository _accounts;
    private readonly IInvoiceRepository _invoices;
    private readonly IDebtRepository _debts;

    public DeleteTransactionUseCase(ITransactionRepository transactionRepository, IUnitOfWork unitOfWork, IAccountRepository accounts, IInvoiceRepository invoices, IDebtRepository debts)
    {
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
        _accounts = accounts;
        _invoices = invoices;
        _debts = debts;
    }

    public async Task<TransactionResponse> ExecuteAsync(GetTransactionRequest request, CancellationToken cancellationToken = default)
    {
        if (request.OwnerScopeId == Guid.Empty || request.TransactionId == Guid.Empty)
            throw new RequestValidationException(new[] { new ValidationError(nameof(request.TransactionId), "OwnerScopeId e TransactionId sao obrigatorios.") });

        var transaction = await _transactionRepository.GetByIdAsync(request.TransactionId, request.OwnerScopeId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Transaction), request.TransactionId);

        if (transaction.TransferGroupId is not null)
            throw new RequestValidationException(new[] { new ValidationError(nameof(request.TransactionId), "Lancamentos de transferencia devem ser tratados como um grupo e nao podem ser excluidos individualmente.") });

        if (transaction.InstallmentCount > 1)
            throw new OperationRejectedException("Parcelas de uma compra não podem ser excluídas individualmente.");
        if (transaction.InvoiceId is Guid invoiceId)
        {
            var invoice = await _invoices.GetByIdAsync(invoiceId, request.OwnerScopeId, cancellationToken);
            if (invoice?.Status != FinanceControl.Domain.Enums.InvoiceStatus.Open)
                throw new OperationRejectedException("Uma compra de fatura fechada não pode ser excluída.");
        }
        if (await _debts.IsPaymentAsync(request.OwnerScopeId, transaction.Id, cancellationToken))
            throw new OperationRejectedException("Este lançamento está vinculado ao pagamento de uma dívida.");

        var account = await _accounts.GetByIdAsync(transaction.AccountId, request.OwnerScopeId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Account), transaction.AccountId);
        transaction.Delete();
        account.RefreshBalance();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return TransactionMapper.ToResponse(transaction);
    }
}
