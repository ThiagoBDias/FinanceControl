using FinanceControl.Domain.Exceptions;
using FinanceControl.Application.DTOs;
using FinanceControl.Application.Interfaces;
using FinanceControl.Application.Validators;
using FinanceControl.Domain.Entities;
using FinanceControl.Domain.Enums;
using FinanceControl.Domain.ValueObjects;

namespace FinanceControl.Application.UseCases.Debts;

public sealed class DebtUseCases(IDebtRepository debts, IAccountRepository accounts,
    ICategoryRepository categories, IUnitOfWork unitOfWork)
{
    public async Task<IReadOnlyList<DebtResponse>> ListAsync(Guid owner, CancellationToken cancellationToken) =>
        (await debts.ListAsync(owner, cancellationToken)).Select(ToResponse).ToList();

    public async Task<DebtResponse> CreateAsync(Guid owner, CreateDebtRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length > 100)
            throw new OperationRejectedException("Informe um nome de até 100 caracteres.");
        if (!Money.IsValid(request.TotalAmount) || request.InstallmentCount is < 1 or > 600 ||
            request.TotalAmount * 100 < request.InstallmentCount || request.FirstDueDate.Year is < 1900 or > 2200)
            throw new OperationRejectedException("Informe valor, vencimento e quantidade de parcelas válidos (1 a 600).");

        if (request.CategoryId is Guid categoryId)
        {
            var category = await categories.GetByIdAsync(categoryId, owner, cancellationToken)
                ?? throw new EntityNotFoundException(nameof(Category), categoryId);
            if (!category.IsActive || category.Type != CategoryType.Expense)
                throw new OperationRejectedException("Selecione uma categoria de despesa ativa.");
        }

        var debt = Debt.Create(owner, request.Name.Trim(), request.TotalAmount, request.InstallmentCount,
            request.FirstDueDate, request.CategoryId);
        debts.Add(debt);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ToResponse(debt);
    }

    public async Task<DebtResponse> PayAsync(Guid owner, Guid id, int number, PayDebtInstallmentRequest request,
        CancellationToken cancellationToken)
    {
        FinancialDates.ValidatePaymentDate(request.PaymentDate);
        var debt = await debts.GetAsync(owner, id, cancellationToken) ?? throw new EntityNotFoundException(nameof(Debt), id);
        var installment = debt.Installments.SingleOrDefault(item => item.Number == number)
            ?? throw new OperationRejectedException("Parcela inexistente.");
        if (installment.PaymentDate is not null)
            throw new OperationRejectedException("Esta parcela já foi paga.");
        var account = await accounts.GetByIdAsync(request.AccountId, owner, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Account), request.AccountId);
        if (account.Type == AccountType.CreditCard)
            throw new OperationRejectedException("Selecione uma conta de saldo para pagar a parcela.");

        Category? category = null;
        if (debt.CategoryId is Guid categoryId)
            category = await categories.GetByIdAsync(categoryId, owner, cancellationToken);
        var transaction = account.RegisterExpense(installment.Amount, installment.DueDate,
            $"{debt.Name} · parcela {number}", category?.IsActive == true ? category : null, installment.DueDate);
        transaction.MarkAsPaid(request.PaymentDate);
        installment.MarkPaid(transaction, request.PaymentDate);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ToResponse(debt);
    }

    private static DebtResponse ToResponse(Debt debt) => new(debt.Id, debt.Name, debt.TotalAmount,
        debt.Installments.Where(i => i.PaymentDate is null).Sum(i => i.Amount), debt.CategoryId,
        debt.Installments.OrderBy(i => i.Number)
            .Select(i => new DebtInstallmentResponse(i.Number, i.DueDate, i.Amount, i.PaymentDate)).ToList());
}
