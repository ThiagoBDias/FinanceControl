using FinanceControl.API;
using FinanceControl.Application.DTOs;
using FinanceControl.Application.UseCases.CreditCards;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceControl.API.Controllers;

[ApiController]
[Authorize]
[Route("api/credit-cards")]
public sealed class CreditCardsController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<AccountResponse>> Create(
        CreateCreditCardRequest request, [FromServices] CreateCreditCardUseCase useCase, CancellationToken cancellationToken)
        => StatusCode(StatusCodes.Status201Created, await useCase.ExecuteAsync(request with { OwnerScopeId = User.GetCurrentUserId() }, cancellationToken));

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AccountResponse>>> List(
        [FromServices] ListCreditCardsUseCase useCase, CancellationToken cancellationToken)
        => Ok(await useCase.ExecuteAsync(new ListCreditCardsRequest(User.GetCurrentUserId()), cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AccountResponse>> Get(
        Guid id, [FromServices] GetCreditCardUseCase useCase, CancellationToken cancellationToken)
        => Ok(await useCase.ExecuteAsync(new GetCreditCardRequest(User.GetCurrentUserId(), id), cancellationToken));
}
