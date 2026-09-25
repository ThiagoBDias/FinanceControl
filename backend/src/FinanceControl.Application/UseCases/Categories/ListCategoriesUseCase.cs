using FinanceControl.Application.DTOs;
using FinanceControl.Application.Interfaces;

namespace FinanceControl.Application.UseCases.Categories;

public sealed class ListCategoriesUseCase : IUseCase<ListCategoriesRequest, IReadOnlyList<CategoryResponse>>
{
    private readonly ICategoryRepository _categoryRepository;

    public ListCategoriesUseCase(ICategoryRepository categoryRepository) => _categoryRepository = categoryRepository;

    public async Task<IReadOnlyList<CategoryResponse>> ExecuteAsync(ListCategoriesRequest request, CancellationToken cancellationToken = default)
    {
        var categories = await _categoryRepository.ListByOwnerAsync(request.OwnerScopeId, cancellationToken);
        return categories.Select(CategoryMapper.ToResponse).ToList();
    }
}
