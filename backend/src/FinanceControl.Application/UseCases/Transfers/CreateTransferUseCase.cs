using FinanceControl.Application.DTOs;
using FinanceControl.Application.Interfaces;
using FinanceControl.Application.Validators;
using FinanceControl.Domain.Entities;

namespace FinanceControl.Application.UseCases.Transfers;

/// <summary>
/// Regras 6/7 do Domain (TransferGroup.Create): so e possivel transferir
/// entre contas do mesmo OwnerScopeId, origem != destino, e as duas
/// Transactions (TransferOut/TransferIn) sao criadas atomicamente. Este
/// caso de uso so carrega as contas e delega tudo ao Domain.
/// </summary>
public sealed class CreateTransferUseCase : IUseCase<CreateTransferRequest, TransferResponse>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransferRepository _transferRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateTransferRequest> _validator;

    public CreateTransferUseCase(
        IAccountRepository accountRepository,
        ITransferRepository transferRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateTransferRequest> validator)
    {
        _accountRepository = accountRepository;
        _transferRepository = transferRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<TransferResponse> ExecuteAsync(CreateTransferRequest request, CancellationToken cancellationToken = default)
    {
        var validation = _validator.Validate(request);
        if (!validation.IsValid)
            throw new RequestValidationException(validation.Errors);

        var source = await _accountRepository.GetByIdAsync(request.SourceAccountId, request.OwnerScopeId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Account), request.SourceAccountId);

        var destination = await _accountRepository.GetByIdAsync(request.DestinationAccountId, request.OwnerScopeId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Account), request.DestinationAccountId);

        FinancialDates.ValidatePaymentDate(request.TransferDate);

        var transferGroup = TransferGroup.Create(
            source, destination, request.Amount, request.TransferDate, request.Description);

        await _transferRepository.AddAsync(transferGroup, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return TransferMapper.ToResponse(transferGroup);
    }
}
