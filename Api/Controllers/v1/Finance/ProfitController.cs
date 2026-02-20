using Microsoft.AspNetCore.Mvc;
using SFManagement.Application.DTOs.Finance;
using SFManagement.Application.Services.Finance;
using SFManagement.Domain.Common;

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
    /// If dates are omitted, defaults to [SystemImplementation.FinanceDataStartDateUtc, today UTC].
    /// managerId accepts either BaseAssetHolderId or PokerManager.Id.
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ProfitSummary), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProfitSummary(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] Guid? managerId = null)
    {
        var (resolvedStartDate, resolvedEndDate) = ResolveDateRange(startDate, endDate);

        if (resolvedStartDate > resolvedEndDate)
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
            resolvedStartDate, resolvedEndDate, managerId);

        var summary = await _profitService.GetProfitSummary(resolvedStartDate, resolvedEndDate, managerId);
        return Ok(summary);
    }

    /// <summary>
    /// Gets detailed direct income transactions for a date range.
    /// </summary>
    [HttpGet("direct-income-details")]
    [ProducesResponseType(typeof(DirectIncomeDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetDirectIncomeDetails(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var (resolvedStartDate, resolvedEndDate) = ResolveDateRange(startDate, endDate);

        if (resolvedStartDate > resolvedEndDate)
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
            resolvedStartDate, resolvedEndDate);

        var details = await _profitService.GetDirectIncomeDetails(resolvedStartDate, resolvedEndDate);
        return Ok(details);
    }

    /// <summary>
    /// Gets profit breakdown by manager for a date range.
    /// If dates are omitted, defaults to [SystemImplementation.FinanceDataStartDateUtc, today UTC].
    /// </summary>
    [HttpGet("by-manager")]
    [ProducesResponseType(typeof(List<ProfitByManager>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProfitByManager(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var (resolvedStartDate, resolvedEndDate) = ResolveDateRange(startDate, endDate);

        if (resolvedStartDate > resolvedEndDate)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid date range",
                Detail = "Start date must be before or equal to end date",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var results = await _profitService.GetProfitByManager(resolvedStartDate, resolvedEndDate);
        return Ok(results);
    }

    /// <summary>
    /// Gets profit breakdown by source for a date range.
    /// </summary>
    [HttpGet("by-source")]
    [ProducesResponseType(typeof(List<ProfitBySource>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProfitBySource(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var (resolvedStartDate, resolvedEndDate) = ResolveDateRange(startDate, endDate);

        if (resolvedStartDate > resolvedEndDate)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid date range",
                Detail = "Start date must be before or equal to end date",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var results = await _profitService.GetProfitBySource(resolvedStartDate, resolvedEndDate);
        return Ok(results);
    }

    private static (DateTime startDate, DateTime endDate) ResolveDateRange(DateTime? startDate, DateTime? endDate)
    {
        var resolvedStart = (startDate ?? SystemImplementation.FinanceDataStartDateUtc).Date;
        var resolvedEnd = (endDate ?? DateTime.UtcNow.Date).Date;
        return (resolvedStart, resolvedEnd);
    }
}
