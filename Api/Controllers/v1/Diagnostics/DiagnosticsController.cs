using Microsoft.AspNetCore.Mvc;
using SFManagement.Application.DTOs.Infrastructure;
using SFManagement.Application.Services.Infrastructure;
using SFManagement.Infrastructure.Authorization;

namespace SFManagement.Api.Controllers.v1.Diagnostics;

[ApiController]
[Route("api/v{version:apiVersion}/diagnostics")]
[ApiVersion("1.0")]
[RequireRole(Auth0Roles.Admin)]
public class DiagnosticsController : ControllerBase
{
    private readonly ICacheMetricsService _cacheMetrics;

    public DiagnosticsController(ICacheMetricsService cacheMetrics)
    {
        _cacheMetrics = cacheMetrics;
    }

    /// <summary>
    /// Returns cache hit/miss statistics grouped by category.
    /// </summary>
    [HttpGet("cache-stats")]
    [ProducesResponseType(typeof(CacheStatistics), StatusCodes.Status200OK)]
    public IActionResult GetCacheStatistics()
    {
        return Ok(_cacheMetrics.GetStatistics());
    }
}
