using Microsoft.AspNetCore.Mvc;
using SFManagement.Application.DTOs.Finance;
using SFManagement.Application.Services.Finance;

namespace SFManagement.Api.Controllers.v1.Finance;

[ApiController]
[Route("api/v{version:apiVersion}/finance/profit")]
[ApiVersion("1.0")]
public class ProfitController : ControllerBase
{
    private readonly IProfitCalculationService _profitService;
    private readonly ILogger<ProfitController> _logger;

    public ProfitController(
        IProfitCalculationService profitService,
        ILogger<ProfitController> logger)
    {
        _profitService = profitService;
        _logger = logger;
    }

    /// <summary>
    /// Gets profit summary for a date range, optionally filtered by manager.
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ProfitSummary), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProfitSummary(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] Guid? managerId = null)
    {
        if (startDate > endDate)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid date range",
                Detail = "Start date must be before or equal to end date",
                Status = StatusCodes.Status400BadRequest
            });
        }

        _logger.LogInformation(
            "Getting profit summary: {StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}, ManagerId={ManagerId}",
            startDate, endDate, managerId);

        var summary = await _profitService.GetProfitSummary(startDate, endDate, managerId);
        return Ok(summary);
    }

    /// <summary>
    /// Gets detailed direct income transactions for a date range.
    /// </summary>
    [HttpGet("direct-income-details")]
    [ProducesResponseType(typeof(DirectIncomeDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetDirectIncomeDetails(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        if (startDate > endDate)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid date range",
                Detail = "Start date must be before or equal to end date",
                Status = StatusCodes.Status400BadRequest
            });
        }

        _logger.LogInformation(
            "Getting direct income details: {StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}",
            startDate, endDate);

        var details = await _profitService.GetDirectIncomeDetails(startDate, endDate);
        return Ok(details);
    }

    /// <summary>
    /// Gets profit breakdown by manager for a date range.
    /// </summary>
    [HttpGet("by-manager")]
    [ProducesResponseType(typeof(List<ProfitByManager>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProfitByManager(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        if (startDate > endDate)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid date range",
                Detail = "Start date must be before or equal to end date",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var results = await _profitService.GetProfitByManager(startDate, endDate);
        return Ok(results);
    }

    /// <summary>
    /// Gets profit breakdown by source for a date range.
    /// </summary>
    [HttpGet("by-source")]
    [ProducesResponseType(typeof(List<ProfitBySource>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProfitBySource(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        if (startDate > endDate)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid date range",
                Detail = "Start date must be before or equal to end date",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var results = await _profitService.GetProfitBySource(startDate, endDate);
        return Ok(results);
    }
}
