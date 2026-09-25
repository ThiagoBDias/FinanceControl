using FinanceControl.Application.DTOs;
using FinanceControl.Application.Interfaces;
using FinanceControl.Application.Validators;

namespace FinanceControl.Application.UseCases.Accounts;

public sealed class UpdateAccountUseCase : IUseCase<UpdateAccountRequest, AccountResponse>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<UpdateAccountRequest> _validator;

    public UpdateAccountUseCase(
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        IValidator<UpdateAccountRequest> validator)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<AccountResponse> ExecuteAsync(UpdateAccountRequest request, CancellationToken cancellationToken = default)
    {
        var validation = _validator.Validate(request);
        if (!validation.IsValid)
            throw new RequestValidationException(validation.Errors);

        var account = await _accountRepository.GetByIdAsync(request.AccountId, request.OwnerScopeId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Domain.Entities.Account), request.AccountId);

        account.Rename(request.Name);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return AccountMapper.ToResponse(account);
    }
}
