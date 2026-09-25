using FinanceControl.Application.DTOs;
using FinanceControl.Application.Interfaces;
using FinanceControl.Application.Validators;

namespace FinanceControl.Application.UseCases.Categories;

public sealed class DeactivateCategoryUseCase : IUseCase<DeactivateCategoryRequest, CategoryResponse>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateCategoryUseCase(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CategoryResponse> ExecuteAsync(DeactivateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        if (request.OwnerScopeId == Guid.Empty || request.CategoryId == Guid.Empty)
            throw new RequestValidationException(new[]
            {
                new ValidationError(nameof(request.CategoryId), "OwnerScopeId e CategoryId sao obrigatorios.")
            });

        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, request.OwnerScopeId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Domain.Entities.Category), request.CategoryId);

        category.Deactivate();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CategoryMapper.ToResponse(category);
    }
}
