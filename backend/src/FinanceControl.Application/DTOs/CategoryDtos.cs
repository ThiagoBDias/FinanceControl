using FinanceControl.Domain.Entities;
using FinanceControl.Domain.Enums;

namespace FinanceControl.Application.DTOs;

public sealed record CreateCategoryRequest(Guid OwnerScopeId, string Name, CategoryType Type);

public sealed record ListCategoriesRequest(Guid OwnerScopeId);

public sealed record UpdateCategoryRequest(Guid OwnerScopeId, Guid CategoryId, string Name);

public sealed record DeactivateCategoryRequest(Guid OwnerScopeId, Guid CategoryId);

public sealed record CategoryResponse(
    Guid Id,
    Guid OwnerScopeId,
    string Name,
    CategoryType Type,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);

internal static class CategoryMapper
{
    public static CategoryResponse ToResponse(Category category) => new(
        category.Id, category.OwnerScopeId, category.Name, category.Type,
        category.IsActive, category.CreatedAt, category.UpdatedAt);
}
