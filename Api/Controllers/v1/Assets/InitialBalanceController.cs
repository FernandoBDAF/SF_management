using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Api.Controllers.Base;
using SFManagement.Application.DTOs.Assets;
using SFManagement.Application.DTOs.CompanyAssets;
using SFManagement.Application.Services.Assets;
using SFManagement.Application.Services.Base;
using SFManagement.Domain.Entities.Support;
using SFManagement.Domain.Enums;
using SFManagement.Domain.Enums.Assets;
using SFManagement.Infrastructure.Authorization;

namespace SFManagement.Api.Controllers.v1.Assets;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[RequireRole(Auth0Roles.Admin)]
public class InitialBalanceController : BaseApiController<InitialBalance, InitialBalanceRequest, InitialBalanceResponse>
{
    private readonly InitialBalanceService _initialBalanceService;
    private readonly IMapper _mapper;

    public InitialBalanceController(InitialBalanceService service, IMapper mapper)
        : base(service, mapper)
    {
        _initialBalanceService = service;
        _mapper = mapper;
    }

    /// <summary>
    /// Sets an initial balance for a specific AssetType
    /// </summary>
    [HttpPost("asset-type")]
    public async Task<ActionResult<InitialBalanceResponse>> SetInitialBalanceForAssetType([FromBody] InitialBalanceRequest request)
    {
        try
        {
            // Validate that AssetType is specified and AssetGroup is not
            if (request.AssetType == 0)
                return BadRequest("AssetType must be specified and cannot be None for this endpoint");
            
            if (request.AssetGroup != 0)
                return BadRequest("AssetGroup should not be specified for this endpoint (it will be automatically set to None)");

            var initialBalance = await _initialBalanceService.SetInitialBalance(
                request.BaseAssetHolderId,
                request.AssetType,
                request.Balance,
                request.BalanceAs,
                request.ConversionRate);

            var response = _mapper.Map<InitialBalanceResponse>(initialBalance);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Sets an initial balance for a specific AssetGroup
    /// </summary>
    [HttpPost("asset-group")]
    public async Task<ActionResult<InitialBalanceResponse>> SetInitialBalanceForAssetGroup([FromBody] InitialBalanceRequest request)
    {
        try
        {
            // Validate that AssetGroup is specified and AssetType is not
            if (request.AssetGroup == 0)
                return BadRequest("AssetGroup must be specified and cannot be None for this endpoint");
            
            if (request.AssetType != 0)
                return BadRequest("AssetType should not be specified for this endpoint (it will be automatically set to None)");

            var initialBalance = await _initialBalanceService.SetInitialBalanceForAssetGroup(
                request.BaseAssetHolderId,
                request.AssetGroup,
                request.Balance,
                request.BalanceAs,
                request.ConversionRate,
                request.Description);

            var response = _mapper.Map<InitialBalanceResponse>(initialBalance);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Sets an initial balance for either a specific AssetType or AssetGroup
    /// This unified endpoint handles both scenarios based on the request data
    /// </summary>
    [HttpPost("unified")]
    public async Task<ActionResult<InitialBalanceResponse>> SetInitialBalanceUnified([FromBody] InitialBalanceRequest request)
    {
        try
        {
            var initialBalance = await _initialBalanceService.SetInitialBalanceUnified(
                request.BaseAssetHolderId,
                request.AssetType != 0 ? request.AssetType : null,
                request.AssetGroup != 0 ? request.AssetGroup : null,
                request.Balance,
                request.BalanceAs,
                request.ConversionRate,
                request.Description);

            var response = _mapper.Map<InitialBalanceResponse>(initialBalance);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets all initial balances for a BaseAssetHolder
    /// </summary>
    [HttpGet("asset-holder/{baseAssetHolderId:guid}")]
    public async Task<ActionResult<List<InitialBalanceResponse>>> GetInitialBalancesForAssetHolder(Guid baseAssetHolderId)
    {
        try
        {
            var initialBalances = await _initialBalanceService.GetInitialBalancesForAssetHolder(baseAssetHolderId);
            var response = _mapper.Map<List<InitialBalanceResponse>>(initialBalances);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets initial balances grouped by AssetType for a BaseAssetHolder
    /// </summary>
    [HttpGet("asset-holder/{baseAssetHolderId:guid}/asset-types")]
    public async Task<ActionResult<Dictionary<AssetType, InitialBalanceResponse>>> GetInitialBalancesByAssetType(Guid baseAssetHolderId)
    {
        try
        {
            var initialBalances = await _initialBalanceService.GetInitialBalancesByAssetType(baseAssetHolderId);
            var response = initialBalances.ToDictionary(
                kvp => kvp.Key,
                kvp => _mapper.Map<InitialBalanceResponse>(kvp.Value));
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets initial balances grouped by AssetGroup for a BaseAssetHolder
    /// </summary>
    [HttpGet("asset-holder/{baseAssetHolderId:guid}/asset-groups")]
    public async Task<ActionResult<Dictionary<AssetGroup, InitialBalanceResponse>>> GetInitialBalancesByAssetGroup(Guid baseAssetHolderId)
    {
        try
        {
            var initialBalances = await _initialBalanceService.GetInitialBalancesByAssetGroup(baseAssetHolderId);
            var response = initialBalances.ToDictionary(
                kvp => kvp.Key,
                kvp => _mapper.Map<InitialBalanceResponse>(kvp.Value));
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Removes an initial balance for a specific AssetType
    /// </summary>
    [HttpDelete("asset-holder/{baseAssetHolderId:guid}/asset-type/{assetType}")]
    public async Task<ActionResult> RemoveInitialBalanceForAssetType(Guid baseAssetHolderId, AssetType assetType)
    {
        try
        {
            var removed = await _initialBalanceService.RemoveInitialBalance(baseAssetHolderId, assetType);
            if (removed)
                return NoContent();
            else
                return NotFound("Initial balance not found");
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Removes an initial balance for a specific AssetGroup
    /// </summary>
    [HttpDelete("asset-holder/{baseAssetHolderId:guid}/asset-group/{assetGroup}")]
    public async Task<ActionResult> RemoveInitialBalanceForAssetGroup(Guid baseAssetHolderId, AssetGroup assetGroup)
    {
        try
        {
            var removed = await _initialBalanceService.RemoveInitialBalanceForAssetGroup(baseAssetHolderId, assetGroup);
            if (removed)
                return NoContent();
            else
                return NotFound("Initial balance not found");
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Validates initial balance data
    /// </summary>
    [HttpPost("validate")]
    public async Task<ActionResult<List<string>>> ValidateInitialBalance([FromBody] InitialBalanceRequest request)
    {
        try
        {
            var errors = await _initialBalanceService.ValidateInitialBalance(
                request.BaseAssetHolderId,
                request.AssetType != 0 ? request.AssetType : null,
                request.AssetGroup != 0 ? request.AssetGroup : null,
                request.Balance,
                request.BalanceAs,
                request.ConversionRate);

            return Ok(errors);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets the effective balance for a specific AssetType
    /// </summary>
    [HttpGet("asset-holder/{baseAssetHolderId:guid}/asset-type/{assetType}/effective-balance")]
    public async Task<ActionResult<decimal?>> GetEffectiveBalanceForAssetType(Guid baseAssetHolderId, AssetType assetType)
    {
        try
        {
            var effectiveBalance = await _initialBalanceService.GetEffectiveBalanceForAssetType(baseAssetHolderId, assetType);
            return Ok(effectiveBalance);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets the effective balance for a specific AssetGroup
    /// </summary>
    [HttpGet("asset-holder/{baseAssetHolderId:guid}/asset-group/{assetGroup}/effective-balance")]
    public async Task<ActionResult<decimal?>> GetEffectiveBalanceForAssetGroup(Guid baseAssetHolderId, AssetGroup assetGroup)
    {
        try
        {
            var effectiveBalance = await _initialBalanceService.GetEffectiveBalanceForAssetGroup(baseAssetHolderId, assetGroup);
            return Ok(effectiveBalance);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets a summary of all initial balances for a BaseAssetHolder
    /// </summary>
    [HttpGet("asset-holder/{baseAssetHolderId:guid}/summary")]
    public async Task<ActionResult<InitialBalanceSummary>> GetInitialBalanceSummary(Guid baseAssetHolderId)
    {
        try
        {
            var summary = await _initialBalanceService.GetInitialBalanceSummary(baseAssetHolderId);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Checks if a BaseAssetHolder has any initial balances
    /// </summary>
    [HttpGet("asset-holder/{baseAssetHolderId:guid}/has-balances")]
    public async Task<ActionResult<bool>> HasInitialBalances(Guid baseAssetHolderId)
    {
        try
        {
            var hasBalances = await _initialBalanceService.HasInitialBalances(baseAssetHolderId);
            return Ok(hasBalances);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets the total count of initial balances for a BaseAssetHolder
    /// </summary>
    [HttpGet("asset-holder/{baseAssetHolderId:guid}/count")]
    public async Task<ActionResult<int>> GetInitialBalanceCount(Guid baseAssetHolderId)
    {
        try
        {
            var count = await _initialBalanceService.GetInitialBalanceCount(baseAssetHolderId);
            return Ok(count);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}