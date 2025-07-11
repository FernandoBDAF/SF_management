using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Enums;
using SFManagement.Exceptions;
using SFManagement.Models.AssetInfrastructure;
using SFManagement.Services;
using SFManagement.ViewModels;
using System.Text.Json;

namespace SFManagement.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}/company/asset-pools")]
[ApiVersion("1.0")]
public class CompanyAssetPoolController : ControllerBase
{
    private readonly AssetPoolService _assetPoolService;
    private readonly AssetPoolValidationService _validationService;
    private readonly IMapper _mapper;
    private readonly ILogger<CompanyAssetPoolController> _logger;

    public CompanyAssetPoolController(
        AssetPoolService assetPoolService,
        AssetPoolValidationService validationService,
        IMapper mapper,
        ILogger<CompanyAssetPoolController> logger)
    {
        _assetPoolService = assetPoolService;
        _validationService = validationService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Gets all company-owned asset pools
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<CompanyAssetPoolResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCompanyAssetPools()
    {
        var requestId = HttpContext.TraceIdentifier;
        _logger.LogInformation("Retrieving all company asset pools - RequestId: {RequestId}", requestId);

        try
        {
            var companyPools = await _assetPoolService.GetCompanyAssetPools();
            var response = new List<CompanyAssetPoolResponse>();

            foreach (var pool in companyPools)
            {
                var poolResponse = _mapper.Map<CompanyAssetPoolResponse>(pool);
                
                // Enrich with calculated metrics
                poolResponse.CurrentBalance = await _assetPoolService.GetAssetPoolBalance(pool.Id);
                poolResponse.TransactionCount = await GetTransactionCount(pool.Id);
                poolResponse.LastTransactionDate = await GetLastTransactionDate(pool.Id);
                
                response.Add(poolResponse);
            }

            _logger.LogInformation("Successfully retrieved {Count} company asset pools - RequestId: {RequestId}", 
                response.Count, requestId);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving company asset pools - RequestId: {RequestId}", requestId);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An error occurred while retrieving company asset pools",
                    Status = StatusCodes.Status500InternalServerError,
                    Extensions = { ["requestId"] = requestId, ["timestamp"] = DateTime.UtcNow }
                });
        }
    }

    /// <summary>
    /// Gets a specific company asset pool by asset type
    /// </summary>
    [HttpGet("{assetType}")]
    [ProducesResponseType(typeof(CompanyAssetPoolResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCompanyAssetPool(AssetType assetType)
    {
        var requestId = HttpContext.TraceIdentifier;
        _logger.LogInformation("Retrieving company asset pool for {AssetType} - RequestId: {RequestId}", 
            assetType, requestId);

        try
        {
            var pool = await _assetPoolService.GetCompanyAssetPoolByType(assetType);
            
            if (pool == null)
            {
                _logger.LogWarning("Company asset pool not found for {AssetType} - RequestId: {RequestId}", 
                    assetType, requestId);
                return NotFound(new ProblemDetails
                {
                    Title = "Company Asset Pool Not Found",
                    Detail = $"No company asset pool found for asset type {assetType}",
                    Status = StatusCodes.Status404NotFound,
                    Extensions = { ["requestId"] = requestId, ["assetType"] = assetType.ToString() }
                });
            }

            var response = _mapper.Map<CompanyAssetPoolResponse>(pool);
            
            // Enrich with calculated metrics
            response.CurrentBalance = await _assetPoolService.GetAssetPoolBalance(pool.Id);
            response.TransactionCount = await GetTransactionCount(pool.Id);
            response.LastTransactionDate = await GetLastTransactionDate(pool.Id);

            _logger.LogInformation("Successfully retrieved company asset pool for {AssetType} - RequestId: {RequestId}", 
                assetType, requestId);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving company asset pool for {AssetType} - RequestId: {RequestId}", 
                assetType, requestId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = $"An error occurred while retrieving company asset pool for {assetType}",
                    Status = StatusCodes.Status500InternalServerError,
                    Extensions = { ["requestId"] = requestId, ["assetType"] = assetType.ToString() }
                });
        }
    }

    /// <summary>
    /// Creates a new company asset pool
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CompanyAssetPoolResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateCompanyAssetPool([FromBody] CompanyAssetPoolRequest request)
    {
        var requestId = HttpContext.TraceIdentifier;
        _logger.LogInformation("Creating company asset pool for {AssetType} - RequestId: {RequestId}", 
            request.AssetType, requestId);

        try
        {
            // Create AssetPool from request
            var assetPool = _mapper.Map<AssetPool>(request);
            
            // Validate using the validation service
            var validationResult = await _validationService.ValidateAssetPoolCreation(assetPool);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation failed for company asset pool creation - RequestId: {RequestId} - Errors: {ValidationErrors}", 
                    requestId, JsonSerializer.Serialize(validationResult.Errors));
                
                var problemDetails = new ValidationProblemDetails();
                foreach (var error in validationResult.Errors)
                {
                    if (!problemDetails.Errors.ContainsKey(error.Field))
                        problemDetails.Errors[error.Field] = new string[] { };
                    
                    var errorList = problemDetails.Errors[error.Field].ToList();
                    errorList.Add(error.Message);
                    problemDetails.Errors[error.Field] = errorList.ToArray();
                }
                
                problemDetails.Title = "Validation Failed";
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Extensions["requestId"] = requestId;
                
                return BadRequest(problemDetails);
            }

            // Create the pool
            var createdPool = await _assetPoolService.CreateCompanyAssetPool(
                request.AssetType, 
                request.Description, 
                request.BusinessJustification);

            var response = _mapper.Map<CompanyAssetPoolResponse>(createdPool);
            
            // Enrich with initial metrics
            response.CurrentBalance = request.InitialBalance ?? 0;
            response.TransactionCount = 0;
            response.LastTransactionDate = null;

            _logger.LogInformation("Successfully created company asset pool for {AssetType} with ID: {PoolId} - RequestId: {RequestId}", 
                request.AssetType, createdPool.Id, requestId);

            return CreatedAtAction(
                nameof(GetCompanyAssetPool), 
                new { assetType = request.AssetType }, 
                response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Business rule violation for company asset pool creation - RequestId: {RequestId} - Message: {Message}", 
                requestId, ex.Message);
            return Conflict(new ProblemDetails
            {
                Title = "Business Rule Violation",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict,
                Extensions = { ["requestId"] = requestId, ["assetType"] = request.AssetType.ToString() }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating company asset pool for {AssetType} - RequestId: {RequestId}", 
                request.AssetType, requestId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An error occurred while creating the company asset pool",
                    Status = StatusCodes.Status500InternalServerError,
                    Extensions = { ["requestId"] = requestId, ["assetType"] = request.AssetType.ToString() }
                });
        }
    }

    /// <summary>
    /// Gets company asset pools summary with metrics
    /// </summary>
    [HttpGet("summary")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any, NoStore = false)]
    [ProducesResponseType(typeof(CompanyAssetPoolSummaryResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCompanyAssetPoolSummary()
    {
        var requestId = HttpContext.TraceIdentifier;
        _logger.LogInformation("Retrieving company asset pools summary - RequestId: {RequestId}", requestId);

        try
        {
            var summary = await _assetPoolService.GetCompanyAssetPoolSummary();

            _logger.LogInformation("Successfully retrieved company asset pools summary - TotalPools: {TotalPools}, TotalBalance: {TotalBalance} - RequestId: {RequestId}", 
                summary.TotalPools, summary.TotalBalance, requestId);

            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving company asset pools summary - RequestId: {RequestId}", requestId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An error occurred while retrieving company asset pools summary",
                    Status = StatusCodes.Status500InternalServerError,
                    Extensions = { ["requestId"] = requestId }
                });
        }
    }

    /// <summary>
    /// Deletes a company asset pool (with validation)
    /// </summary>
    [HttpDelete("{assetType}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteCompanyAssetPool(AssetType assetType)
    {
        var requestId = HttpContext.TraceIdentifier;
        _logger.LogInformation("Deleting company asset pool for {AssetType} - RequestId: {RequestId}", 
            assetType, requestId);

        try
        {
            var pool = await _assetPoolService.GetCompanyAssetPoolByType(assetType);
            
            if (pool == null)
            {
                _logger.LogWarning("Company asset pool not found for deletion - {AssetType} - RequestId: {RequestId}", 
                    assetType, requestId);
                return NotFound(new ProblemDetails
                {
                    Title = "Company Asset Pool Not Found",
                    Detail = $"No company asset pool found for asset type {assetType}",
                    Status = StatusCodes.Status404NotFound,
                    Extensions = { ["requestId"] = requestId, ["assetType"] = assetType.ToString() }
                });
            }

            // Validate deletion
            var validationResult = await _validationService.ValidateAssetPoolDeletion(pool.Id);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Deletion validation failed for company asset pool - RequestId: {RequestId} - Errors: {ValidationErrors}", 
                    requestId, JsonSerializer.Serialize(validationResult.Errors));
                
                var firstError = validationResult.Errors.First();
                return Conflict(new ProblemDetails
                {
                    Title = "Cannot Delete Asset Pool",
                    Detail = firstError.Message,
                    Status = StatusCodes.Status409Conflict,
                    Extensions = { 
                        ["requestId"] = requestId, 
                        ["assetType"] = assetType.ToString(),
                        ["errorCode"] = firstError.Code
                    }
                });
            }

            await _assetPoolService.Delete(pool.Id);

            _logger.LogInformation("Successfully deleted company asset pool for {AssetType} - RequestId: {RequestId}", 
                assetType, requestId);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting company asset pool for {AssetType} - RequestId: {RequestId}", 
                assetType, requestId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An error occurred while deleting the company asset pool",
                    Status = StatusCodes.Status500InternalServerError,
                    Extensions = { ["requestId"] = requestId, ["assetType"] = assetType.ToString() }
                });
        }
    }

    /// <summary>
    /// Gets company asset pools analytics by period (month/year)
    /// </summary>
    [HttpGet("analytics")]
    [ResponseCache(Duration = 600, Location = ResponseCacheLocation.Any, NoStore = false)] // 10 minutes cache
    [ProducesResponseType(typeof(CompanyAssetPoolAnalyticsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCompanyAssetPoolAnalytics([FromQuery] CompanyAssetPoolAnalyticsRequest request)
    {
        var requestId = HttpContext.TraceIdentifier;
        _logger.LogInformation("Retrieving company asset pools analytics for {Year}/{Month} - RequestId: {RequestId}", 
            request.Year, request.Month, requestId);

        try
        {
            // Validate the request
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid analytics request - RequestId: {RequestId} - Errors: {ValidationErrors}", 
                    requestId, JsonSerializer.Serialize(ModelState));
                
                return BadRequest(new ValidationProblemDetails(ModelState)
                {
                    Title = "Invalid Analytics Request",
                    Status = StatusCodes.Status400BadRequest,
                    Extensions = { ["requestId"] = requestId }
                });
            }

            var analytics = await _assetPoolService.GetCompanyAssetPoolAnalytics(
                request.Year,
                request.Month,
                request.IncludeTransactions,
                request.TransactionLimit);

            _logger.LogInformation("Successfully retrieved company asset pools analytics - Period: {PeriodName}, ActivePools: {ActivePools}, TotalBalance: {TotalBalance} - RequestId: {RequestId}", 
                analytics.Period.PeriodName, analytics.Summary.ActivePoolsCount, analytics.Summary.TotalEndingBalance, requestId);

            return Ok(analytics);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid analytics parameters - RequestId: {RequestId} - Message: {Message}", 
                requestId, ex.Message);
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Parameters",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest,
                Extensions = { ["requestId"] = requestId }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving company asset pools analytics for {Year}/{Month} - RequestId: {RequestId}", 
                request.Year, request.Month, requestId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An error occurred while retrieving company asset pools analytics",
                    Status = StatusCodes.Status500InternalServerError,
                    Extensions = { ["requestId"] = requestId, ["year"] = request.Year, ["month"] = request.Month }
                });
        }
    }

    #region Private Helper Methods

    private async Task<int> GetTransactionCount(Guid assetPoolId)
    {
        // This would be better as a method in AssetPoolService, but for now we'll use a simple approach
        try
        {
            // For now, return 0 - this should be implemented properly in AssetPoolService
            return 0;
        }
        catch
        {
            return 0;
        }
    }

    private async Task<DateTime?> GetLastTransactionDate(Guid assetPoolId)
    {
        // This would be better as a method in AssetPoolService, but for now we'll use a simple approach
        try
        {
            // For now, return null - this should be implemented properly in AssetPoolService
            return null;
        }
        catch
        {
            return null;
        }
    }

    #endregion
} 