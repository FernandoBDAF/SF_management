using Microsoft.AspNetCore.Mvc;
using SFManagement.Application.DTOs.Finance;
using SFManagement.Application.Services.Finance;
using SFManagement.Domain.Common;
using SFManagement.Infrastructure.Authorization;

namespace SFManagement.Api.Controllers.v1.Finance;

[ApiController]
[Route("api/v{version:apiVersion}/finance/profit")]
[ApiVersion("1.0")]
[RequirePermission(Auth0Permissions.ReadFinancialData)]
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

    /// <summary>
    /// Gets itemized rate fee transactions for a date range.
    /// Each item shows the transaction, fee calculation, and BRL conversion.
    /// </summary>
    [HttpGet("rate-fee-details")]
    [ProducesResponseType(typeof(RateFeeDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetRateFeeDetails(
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
            "Getting rate fee details: {StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}",
            resolvedStartDate, resolvedEndDate);

        var details = await _profitService.GetRateFeeDetails(resolvedStartDate, resolvedEndDate);
        return Ok(details);
    }

    /// <summary>
    /// Gets itemized rake commission settlements for a date range.
    /// Each item shows the settlement, rake calculation, and BRL conversion.
    /// </summary>
    [HttpGet("rake-commission-details")]
    [ProducesResponseType(typeof(RakeCommissionDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetRakeCommissionDetails(
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
            "Getting rake commission details: {StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}",
            resolvedStartDate, resolvedEndDate);

        var details = await _profitService.GetRakeCommissionDetails(resolvedStartDate, resolvedEndDate);
        return Ok(details);
    }

    /// <summary>
    /// Gets itemized spread profit transactions for a date range.
    /// Each item shows the sale transaction, AvgRate cost basis, and BRL profit.
    /// </summary>
    [HttpGet("spread-details")]
    [ProducesResponseType(typeof(SpreadProfitDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSpreadProfitDetails(
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
            "Getting spread profit details: {StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}",
            resolvedStartDate, resolvedEndDate);

        var details = await _profitService.GetSpreadProfitDetails(resolvedStartDate, resolvedEndDate);
        return Ok(details);
    }

    /// <summary>
    /// Gets the AvgRate (Cotação) for each poker manager at a given date.
    /// RakeOverrideCommission managers always return 1.
    /// Spread managers return the AvgRate from the cost basis service.
    /// </summary>
    [HttpGet("avg-rates")]
    [ProducesResponseType(typeof(Dictionary<Guid, decimal>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetManagerAvgRates(
        [FromQuery] DateTime? asOfDate = null)
    {
        var resolvedDate = (asOfDate ?? DateTime.UtcNow.Date).Date;

        _logger.LogInformation("Getting manager avg rates at {Date:yyyy-MM-dd}", resolvedDate);

        var rates = await _profitService.GetManagerAvgRates(resolvedDate);
        return Ok(rates);
    }

    private static (DateTime startDate, DateTime endDate) ResolveDateRange(DateTime? startDate, DateTime? endDate)
    {
        var resolvedStart = (startDate ?? SystemImplementation.FinanceDataStartDateUtc).Date;
        var resolvedEnd = (endDate ?? DateTime.UtcNow.Date).Date;
        return (resolvedStart, resolvedEnd);
    }
}
