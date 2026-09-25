using FinanceControl.Application.DTOs;
using FinanceControl.Application.UseCases.Categories;
using FinanceControl.API;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceControl.API.Controllers;

[ApiController]
[Authorize]
[Route("api/categories")]
public sealed class CategoriesController : ControllerBase
{
    private readonly CreateCategoryUseCase _create;
    private readonly ListCategoriesUseCase _list;
    private readonly UpdateCategoryUseCase _update;
    private readonly DeactivateCategoryUseCase _deactivate;

    public CategoriesController(
        CreateCategoryUseCase create,
        ListCategoriesUseCase list,
        UpdateCategoryUseCase update,
        DeactivateCategoryUseCase deactivate)
    {
        _create = create;
        _list = list;
        _update = update;
        _deactivate = deactivate;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CategoryResponse>> Create(
        [FromBody] CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _create.ExecuteAsync(request with { OwnerScopeId = User.GetCurrentUserId() }, cancellationToken);
        return CreatedAtAction(nameof(List), new { ownerScopeId = response.OwnerScopeId }, response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CategoryResponse>>> List(CancellationToken cancellationToken)
    {
        var ownerScopeId = User.GetCurrentUserId();
        var response = await _list.ExecuteAsync(
            new ListCategoriesRequest(ownerScopeId),
            cancellationToken);

        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryResponse>> Update(
        Guid id,
        [FromBody] UpdateCategoryBody body,
        CancellationToken cancellationToken)
    {
        var response = await _update.ExecuteAsync(
            new UpdateCategoryRequest(User.GetCurrentUserId(), id, body.Name),
            cancellationToken);

        return Ok(response);
    }

    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryResponse>> Deactivate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var response = await _deactivate.ExecuteAsync(
            new DeactivateCategoryRequest(User.GetCurrentUserId(), id),
            cancellationToken);

        return Ok(response);
    }
}

public sealed record UpdateCategoryBody(string Name);
