using SFManagement.Application.DTOs.Statements;
using SFManagement.Application.DTOs.Common;
using SFManagement.Application.DTOs.AssetHolders;
using SFManagement.Application.DTOs.Assets;
using SFManagement.Application.Services.Base;
using SFManagement.Application.Services.Assets;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Domain.Enums;
using SFManagement.Domain.Exceptions;
using SFManagement.Domain.Interfaces;
using SFManagement.Domain.Common;
using SFManagement.Domain.Entities.Assets;
using SFManagement.Domain.Entities.AssetHolders;
using SFManagement.Application.Services;
using SFManagement.Application.DTOs;
using System.Text.Json;
using SFManagement.Domain.Enums.Assets;

namespace SFManagement.Api.Controllers.Base;

/// <summary>
/// Base controller for asset holder entities (Client, Bank, Member, PokerManager)
/// </summary>
/// <typeparam name="TEntity">The asset holder entity type</typeparam>
/// <typeparam name="TRequest">The request model type</typeparam>
/// <typeparam name="TResponse">The response model type</typeparam>
public class BaseAssetHolderController<TEntity, TRequest, TResponse> : BaseApiController<TEntity, TRequest, TResponse>
    where TEntity : BaseDomain, IAssetHolder
    where TRequest : BaseAssetHolderRequest
    where TResponse : BaseResponse
{
    protected readonly BaseAssetHolderService<TEntity> _assetHolderService;
    protected readonly WalletIdentifierService _walletIdentifierService;
    protected readonly IMapper _mapper;
    protected readonly ILogger<BaseAssetHolderController<TEntity, TRequest, TResponse>> _logger;

    public BaseAssetHolderController(BaseAssetHolderService<TEntity> service, WalletIdentifierService walletIdentifierService, 
    IMapper mapper, ILogger<BaseAssetHolderController<TEntity, TRequest, TResponse>> logger) 
        : base(service, mapper)
    {
        _assetHolderService = service;
        _walletIdentifierService = walletIdentifierService;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet]
    [Route("")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public override async Task<IActionResult> Get()
    {
        var requestId = HttpContext.TraceIdentifier;
        var entityType = typeof(TEntity).Name;
        
        _logger.LogInformation("Retrieving all {EntityType} entities - RequestId: {RequestId}", entityType, requestId);
        
        try
        {
            var entities = await _assetHolderService.List();
            
            _logger.LogInformation("Successfully retrieved {Count} {EntityType} entities - RequestId: {RequestId}", 
                entities.Count, entityType, requestId);
            
            var response = _mapper.Map<List<TResponse>>(entities);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving {EntityType} entities - RequestId: {RequestId}", 
                entityType, requestId);
            return HandleGenericException("retrieving");
        }
    }

    [HttpGet]
    [Route("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public override async Task<IActionResult> Get(Guid id)
    {
        var requestId = HttpContext.TraceIdentifier;
        var entityType = typeof(TEntity).Name;
        
        _logger.LogInformation("Retrieving {EntityType} {EntityId} - RequestId: {RequestId}", entityType, id, requestId);
        
        try
        {
            var entity = await _assetHolderService.Get(id);
            if (entity == null)
            {
                _logger.LogWarning("{EntityType} {EntityId} not found - RequestId: {RequestId}", 
                    entityType, id, requestId);
                return NotFound();
            }
            
            _logger.LogInformation("Successfully retrieved {EntityType} {EntityId} - RequestId: {RequestId}", 
                entityType, id, requestId);
            
            var response = _mapper.Map<TResponse>(entity);
            return Ok(response);
        }
        catch (EntityNotFoundException ex)
        {
            _logger.LogWarning("{EntityType} {EntityId} not found - RequestId: {RequestId}", 
                entityType, id, requestId);
            return HandleEntityNotFoundException(ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving {EntityType} {EntityId} - RequestId: {RequestId}", 
                entityType, id, requestId);
            return HandleGenericException("retrieving");
        }
    }

    /// <summary>
    /// Creates a new asset holder with comprehensive validation
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public override async Task<IActionResult> Post([FromBody] TRequest request)
    {
        var requestId = HttpContext.TraceIdentifier;
        var entityType = typeof(TEntity).Name;
        
        _logger.LogInformation("Creating {EntityType} - RequestId: {RequestId}", entityType, requestId);
        
        try
        {
            var entity = await CreateEntityFromRequest(request);
            var response = _mapper.Map<TResponse>(entity);
            
            _logger.LogInformation("Successfully created {EntityType} with ID: {EntityId} - RequestId: {RequestId}", 
                entityType, response.Id, requestId);
            
            return CreatedAtAction(nameof(Get), new { id = response.Id }, response);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation failed for {EntityType} creation - RequestId: {RequestId} - Errors: {ValidationErrors}", 
                entityType, requestId, JsonSerializer.Serialize(ex.ValidationErrors));
            return HandleValidationException(ex);
        }
        catch (DuplicateEntityException ex)
        {
            _logger.LogWarning("Duplicate entity detected for {EntityType} creation - RequestId: {RequestId} - Message: {Message}", 
                entityType, requestId, ex.Message);
            return HandleDuplicateEntityException(ex);
        }
        catch (BusinessException ex)
        {
            _logger.LogWarning("Business rule violation for {EntityType} creation - RequestId: {RequestId} - Message: {Message}", 
                entityType, requestId, ex.Message);
            return HandleBusinessException(ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating {EntityType} - RequestId: {RequestId}", entityType, requestId);
            return HandleGenericException("creating");
        }
    }

    /// <summary>
    /// Updates an existing asset holder
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public override async Task<IActionResult> Put(Guid id, [FromBody] TRequest request)
    {
        var requestId = HttpContext.TraceIdentifier;
        var entityType = typeof(TEntity).Name;
        
        _logger.LogInformation("Updating {EntityType} {EntityId} - RequestId: {RequestId}", entityType, id, requestId);
        
        try
        {
            var entity = await UpdateEntityFromRequest(id, request);
            var response = _mapper.Map<TResponse>(entity);
            
            _logger.LogInformation("Successfully updated {EntityType} {EntityId} - RequestId: {RequestId}", 
                entityType, id, requestId);
            
            return Ok(response);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation failed for {EntityType} {EntityId} update - RequestId: {RequestId} - Errors: {ValidationErrors}", 
                entityType, id, requestId, JsonSerializer.Serialize(ex.ValidationErrors));
            return HandleValidationException(ex);
        }
        catch (EntityNotFoundException ex)
        {
            _logger.LogWarning("{EntityType} {EntityId} not found for update - RequestId: {RequestId}", 
                entityType, id, requestId);
            return HandleEntityNotFoundException(ex);
        }
        catch (DuplicateEntityException ex)
        {
            _logger.LogWarning("Duplicate entity detected for {EntityType} {EntityId} update - RequestId: {RequestId} - Message: {Message}", 
                entityType, id, requestId, ex.Message);
            return HandleDuplicateEntityException(ex);
        }
        catch (BusinessException ex)
        {
            _logger.LogWarning("Business rule violation for {EntityType} {EntityId} update - RequestId: {RequestId} - Message: {Message}", 
                entityType, id, requestId, ex.Message);
            return HandleBusinessException(ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating {EntityType} {EntityId} - RequestId: {RequestId}", 
                entityType, id, requestId);
            return HandleGenericException("updating");
        }
    }

    /// <summary>
    /// Deletes an asset holder with business rule validation
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public override async Task<IActionResult> Delete(Guid id)
    {
        var requestId = HttpContext.TraceIdentifier;
        var entityType = typeof(TEntity).Name;
        
        _logger.LogInformation("Deleting {EntityType} {EntityId} - RequestId: {RequestId}", entityType, id, requestId);
        
        try
        {
            await _assetHolderService.DeleteWithValidation(id);
            
            _logger.LogInformation("Successfully deleted {EntityType} {EntityId} - RequestId: {RequestId}", 
                entityType, id, requestId);
            
            return NoContent();
        }
        catch (EntityNotFoundException ex)
        {
            _logger.LogWarning("{EntityType} {EntityId} not found for deletion - RequestId: {RequestId}", 
                entityType, id, requestId);
            return HandleEntityNotFoundException(ex);
        }
        catch (BusinessRuleException ex)
        {
            _logger.LogWarning("Business rule violation for {EntityType} {EntityId} deletion - RequestId: {RequestId} - Message: {Message}", 
                entityType, id, requestId, ex.Message);
            return HandleBusinessRuleException(ex);
        }
        catch (BusinessException ex)
        {
            _logger.LogWarning("Business error for {EntityType} {EntityId} deletion - RequestId: {RequestId} - Message: {Message}", 
                entityType, id, requestId, ex.Message);
            return HandleBusinessException(ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting {EntityType} {EntityId} - RequestId: {RequestId}", 
                entityType, id, requestId);
            return HandleGenericException("deleting");
        }
    }

    [HttpGet]
    [Route("wallet-identifiers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public virtual async Task<IActionResult> 
    GetWalletIdentifiers([FromQuery] AssetGroup? assetGroup, [FromQuery] AssetType? assetType)
    {
        var requestId = HttpContext.TraceIdentifier;
        var entityType = typeof(TEntity).Name;
        
        _logger.LogInformation("Retrieving wallet identifiers for {EntityType} - AssetGroup: {AssetGroup}, AssetType: {AssetType} - RequestId: {RequestId}", 
            entityType, assetGroup, assetType, requestId);
        
        try
        {
            var walletIdentifiers = await _walletIdentifierService.GetByAssetHolderTypeFiltered(typeof(TEntity).Name, assetType, assetGroup);

            _logger.LogInformation("Successfully retrieved {Count} wallet identifiers for {EntityType} - RequestId: {RequestId}", 
                walletIdentifiers.Count, entityType, requestId);

            var response = _mapper.Map<List<WalletIdentifierResponse>>(walletIdentifiers);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving wallet identifiers for {EntityType} - RequestId: {RequestId}", 
                entityType, requestId);
            return HandleGenericException("retrieving wallet identifiers for");
        }
    }

    [HttpGet]
    [Route("{id}/wallet-identifiers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public virtual async Task<IActionResult> 
    GetWalletIdentifiers(Guid id, [FromQuery] AssetGroup? assetGroup, [FromQuery] AssetType? assetType)
    {
        var requestId = HttpContext.TraceIdentifier;
        var entityType = typeof(TEntity).Name;
        
        _logger.LogInformation("Retrieving wallet identifiers for {EntityType} {EntityId} - AssetGroup: {AssetGroup}, AssetType: {AssetType} - RequestId: {RequestId}", 
            entityType, id, assetGroup, assetType, requestId);
        
        try
        {
            var walletIdentifiers = await _walletIdentifierService.GetByAssetHolderAndFilters(id, assetType, assetGroup);

            _logger.LogInformation("Successfully retrieved {Count} wallet identifiers for {EntityType} {EntityId} - RequestId: {RequestId}", 
                walletIdentifiers.Count, entityType, id, requestId);

            var response = _mapper.Map<List<WalletIdentifierResponse>>(walletIdentifiers);
            return Ok(response);
        }
        catch (EntityNotFoundException ex)
        {
            _logger.LogWarning("{EntityType} {EntityId} not found for wallet identifier retrieval - RequestId: {RequestId}", 
                entityType, id, requestId);
            return HandleEntityNotFoundException(ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving wallet identifiers for {EntityType} {EntityId} - RequestId: {RequestId}", 
                entityType, id, requestId);
            return HandleGenericException("retrieving wallet identifiers for");
        }
    }

    /// <summary>
    /// Gets asset holder statistics including balance and transaction information
    /// </summary>
    [HttpGet("{id}/statistics")]
    [ProducesResponseType(typeof(AssetHolderStatistics), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public virtual async Task<IActionResult> GetStatistics(Guid id)
    {
        var requestId = HttpContext.TraceIdentifier;
        var entityType = typeof(TEntity).Name;
        
        _logger.LogInformation("Retrieving statistics for {EntityType} {EntityId} - RequestId: {RequestId}", 
            entityType, id, requestId);
        
        try
        {
            var statistics = await _assetHolderService.GetAssetHolderStatistics(id);
            
            _logger.LogInformation("Successfully retrieved statistics for {EntityType} {EntityId} - RequestId: {RequestId}", 
                entityType, id, requestId);
            
            return Ok(statistics);
        }
        catch (EntityNotFoundException ex)
        {
            _logger.LogWarning("{EntityType} {EntityId} not found for statistics - RequestId: {RequestId}", 
                entityType, id, requestId);
            return HandleEntityNotFoundException(ex);
        }
        catch (BusinessException ex)
        {
            _logger.LogWarning("Business error retrieving statistics for {EntityType} {EntityId} - RequestId: {RequestId} - Message: {Message}", 
                entityType, id, requestId, ex.Message);
            return HandleBusinessException(ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving statistics for {EntityType} {EntityId} - RequestId: {RequestId}", 
                entityType, id, requestId);
            return HandleGenericException("retrieving statistics for");
        }
    }

    /// <summary>
    /// Checks if an asset holder can be deleted
    /// </summary>
    [HttpGet("{id}/can-delete")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public virtual async Task<IActionResult> CanDelete(Guid id)
    {
        var requestId = HttpContext.TraceIdentifier;
        var entityType = typeof(TEntity).Name;
        
        _logger.LogInformation("Checking deletion feasibility for {EntityType} {EntityId} - RequestId: {RequestId}", 
            entityType, id, requestId);
        
        try
        {
            var canDelete = await _assetHolderService.CanDelete(id);
            
            _logger.LogInformation("Successfully checked deletion feasibility for {EntityType} {EntityId} - Result: {CanDelete} - RequestId: {RequestId}", 
                entityType, id, canDelete, requestId);
            
            return Ok(canDelete);
        }
        catch (EntityNotFoundException ex)
        {
            _logger.LogWarning("{EntityType} {EntityId} not found for deletion check - RequestId: {RequestId}", 
                entityType, id, requestId);
            return HandleEntityNotFoundException(ex);
        }
        catch (BusinessException ex)
        {
            _logger.LogWarning("Business error checking deletion feasibility for {EntityType} {EntityId} - RequestId: {RequestId} - Message: {Message}", 
                entityType, id, requestId, ex.Message);
            return HandleBusinessException(ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error checking deletion feasibility for {EntityType} {EntityId} - RequestId: {RequestId}", 
                entityType, id, requestId);
            return HandleGenericException("checking deletion feasibility for");
        }
    }

    /// <summary>
    /// Gets balance by asset type for the asset holder
    /// </summary>
    [HttpGet("{id}/balance")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, NoStore = false)]
    [ProducesResponseType(typeof(Dictionary<string, decimal>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public virtual async Task<IActionResult> GetBalance(Guid id)
    {
        var requestId = HttpContext.TraceIdentifier;
        var entityType = typeof(TEntity).Name;
        
        _logger.LogInformation("Retrieving balance for {EntityType} {EntityId} - RequestId: {RequestId}", 
            entityType, id, requestId);
        
        try
        {
            var balances = await _assetHolderService.GetBalancesByAssetType(id);
            
            _logger.LogInformation("Successfully retrieved balance for {EntityType} {EntityId} - AssetTypes: {AssetTypeCount} - RequestId: {RequestId}", 
                entityType, id, balances.Count, requestId);
            
            return Ok(balances);
        }
        catch (EntityNotFoundException ex)
        {
            _logger.LogWarning("{EntityType} {EntityId} not found for balance retrieval - RequestId: {RequestId}", 
                entityType, id, requestId);
            return HandleEntityNotFoundException(ex);
        }
        catch (BusinessException ex)
        {
            _logger.LogWarning("Business error retrieving balance for {EntityType} {EntityId} - RequestId: {RequestId} - Message: {Message}", 
                entityType, id, requestId, ex.Message);
            return HandleBusinessException(ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving balance for {EntityType} {EntityId} - RequestId: {RequestId}", 
                entityType, id, requestId);
            return HandleGenericException("retrieving balance for");
        }
    }

    /// <summary>
    /// Gets transaction statement for the asset holder
    /// </summary>
    [HttpGet("{id}/transactions")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, NoStore = false)]
    [ProducesResponseType(typeof(StatementAssetHolderWithTransactions), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public virtual async Task<IActionResult> GetTransactions(Guid id)
    {
        var requestId = HttpContext.TraceIdentifier;
        var entityType = typeof(TEntity).Name;
        
        _logger.LogInformation("Retrieving transactions for {EntityType} {EntityId} - RequestId: {RequestId}", 
            entityType, id, requestId);
        
        try
        {
            var statement = await _assetHolderService.GetTransactionsStatementForAssetHolder(id);
            
            _logger.LogInformation("Successfully retrieved transactions for {EntityType} {EntityId} - TransactionCount: {TransactionCount} - RequestId: {RequestId}", 
                entityType, id, statement.Transactions.Length, requestId);
            
            return Ok(statement);
        }
        catch (EntityNotFoundException ex)
        {
            _logger.LogWarning("{EntityType} {EntityId} not found for transaction retrieval - RequestId: {RequestId}", 
                entityType, id, requestId);
            return HandleEntityNotFoundException(ex);
        }
        catch (BusinessException ex)
        {
            _logger.LogWarning("Business error retrieving transactions for {EntityType} {EntityId} - RequestId: {RequestId} - Message: {Message}", 
                entityType, id, requestId, ex.Message);
            return HandleBusinessException(ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving transactions for {EntityType} {EntityId} - RequestId: {RequestId}", 
                entityType, id, requestId);
            return HandleGenericException("retrieving transactions for");
        }
    }

    #region Protected Virtual Methods for Customization

    /// <summary>
    /// Creates an entity from request - override in derived controllers for entity-specific logic
    /// </summary>
    protected virtual Task<TEntity> CreateEntityFromRequest(TRequest request)
    {
        throw new NotImplementedException($"CreateEntityFromRequest must be implemented in {GetType().Name}");
    }

    /// <summary>
    /// Updates an entity from request - override in derived controllers for entity-specific logic
    /// </summary>
    protected virtual Task<TEntity> UpdateEntityFromRequest(Guid id, TRequest request)
    {
        throw new NotImplementedException($"UpdateEntityFromRequest must be implemented in {GetType().Name}");
    }

    #endregion

    #region Error Handling Methods

    protected IActionResult HandleValidationException(ValidationException ex)
    {
        var problemDetails = new ValidationProblemDetails();
        
        foreach (var error in ex.ValidationErrors)
        {
            if (!problemDetails.Errors.ContainsKey(error.Field))
            {
                problemDetails.Errors[error.Field] = new string[] { };
            }
            var errorList = problemDetails.Errors[error.Field].ToList();
            errorList.Add(error.Message);
            problemDetails.Errors[error.Field] = errorList.ToArray();
        }
        
        problemDetails.Title = "Validation Failed";
        problemDetails.Status = StatusCodes.Status400BadRequest;
        problemDetails.Detail = ex.Message;
        problemDetails.Extensions["code"] = ex.Code;
        problemDetails.Extensions["requestId"] = HttpContext.TraceIdentifier;
        problemDetails.Extensions["timestamp"] = DateTime.UtcNow;
        
        // Log detailed validation error information
        _logger.LogWarning("Validation Exception Details - RequestId: {RequestId} - ValidationErrors: {ValidationErrors} - RequestBody: {RequestBody}", 
            HttpContext.TraceIdentifier,
            JsonSerializer.Serialize(ex.ValidationErrors),
            GetRequestBodyForLogging());
        
        return BadRequest(problemDetails);
    }

    protected IActionResult HandleEntityNotFoundException(EntityNotFoundException ex)
    {
        var problemDetails = new ProblemDetails
        {
            Title = "Entity Not Found",
            Status = StatusCodes.Status404NotFound,
            Detail = ex.Message,
            Extensions = { 
                ["code"] = ex.Code, 
                ["data"] = ex.Data,
                ["requestId"] = HttpContext.TraceIdentifier,
                ["timestamp"] = DateTime.UtcNow
            }
        };
        
        return NotFound(problemDetails);
    }

    protected IActionResult HandleDuplicateEntityException(DuplicateEntityException ex)
    {
        var problemDetails = new ProblemDetails
        {
            Title = "Duplicate Entity",
            Status = StatusCodes.Status409Conflict,
            Detail = ex.Message,
            Extensions = { 
                ["code"] = ex.Code, 
                ["data"] = ex.Data,
                ["requestId"] = HttpContext.TraceIdentifier,
                ["timestamp"] = DateTime.UtcNow
            }
        };
        
        return Conflict(problemDetails);
    }

    protected IActionResult HandleBusinessRuleException(BusinessRuleException ex)
    {
        var problemDetails = new ProblemDetails
        {
            Title = "Business Rule Violation",
            Status = StatusCodes.Status409Conflict,
            Detail = ex.Message,
            Extensions = { 
                ["code"] = ex.Code, 
                ["ruleName"] = ex.RuleName,
                ["requestId"] = HttpContext.TraceIdentifier,
                ["timestamp"] = DateTime.UtcNow
            }
        };
        
        return Conflict(problemDetails);
    }

    protected IActionResult HandleBusinessException(BusinessException ex)
    {
        var problemDetails = new ProblemDetails
        {
            Title = "Business Error",
            Status = StatusCodes.Status400BadRequest,
            Detail = ex.Message,
            Extensions = { 
                ["code"] = ex.Code, 
                ["data"] = ex.Data,
                ["requestId"] = HttpContext.TraceIdentifier,
                ["timestamp"] = DateTime.UtcNow
            }
        };
        
        return BadRequest(problemDetails);
    }

    protected IActionResult HandleGenericException(string operation)
    {
        var problemDetails = new ProblemDetails
        {
            Title = "Internal Server Error",
            Status = StatusCodes.Status500InternalServerError,
            Detail = $"An unexpected error occurred while {operation} the {typeof(TEntity).Name.ToLower()}",
            Extensions = {
                ["requestId"] = HttpContext.TraceIdentifier,
                ["timestamp"] = DateTime.UtcNow
            }
        };
        
        return StatusCode(StatusCodes.Status500InternalServerError, problemDetails);
    }

    private string GetRequestBodyForLogging()
    {
        try
        {
            HttpContext.Request.Body.Position = 0;
            using var reader = new StreamReader(HttpContext.Request.Body);
            return reader.ReadToEnd();
        }
        catch
        {
            return "[Unable to read request body]";
        }
    }

    #endregion
} 