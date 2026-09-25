using FinanceControl.API;
using FinanceControl.Application.DTOs;
using FinanceControl.Application.UseCases.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceControl.API.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public sealed class DashboardController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<DashboardResponse>> Get(
        [FromQuery] int? year,
        [FromQuery] int? month,
        [FromServices] GetDashboardUseCase useCase,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var selectedYear = year ?? now.Year;
        var selectedMonth = month ?? now.Month;
        return Ok(await useCase.ExecuteAsync(
            new DashboardRequest(User.GetCurrentUserId(), selectedYear, selectedMonth), cancellationToken));
    }
}
