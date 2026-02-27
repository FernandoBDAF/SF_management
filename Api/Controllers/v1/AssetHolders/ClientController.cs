using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Api.Controllers.Base;
using SFManagement.Application.DTOs.AssetHolders;
using SFManagement.Application.DTOs.Transactions;
using SFManagement.Application.Services.AssetHolders;
using SFManagement.Application.Services.Assets;
using SFManagement.Application.Services.Transactions;
using SFManagement.Domain.Entities.AssetHolders;
using SFManagement.Domain.Entities.Transactions;
using SFManagement.Infrastructure.Authorization;

namespace SFManagement.Api.Controllers.v1.AssetHolders;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[RequirePermission(Auth0Permissions.ReadClients)]
public class ClientController : BaseAssetHolderController<Client, ClientRequest, ClientResponse>
{
    private readonly ClientService _clientService;
    private readonly FiatAssetTransactionService _fiatAssetTransactionService;

    public ClientController(
        ClientService service, 
        FiatAssetTransactionService fiatAssetTransactionService,
        IMapper mapper,
        ILogger<BaseAssetHolderController<Client, ClientRequest, ClientResponse>> logger,
        WalletIdentifierService walletIdentifierService) 
        : base(service, walletIdentifierService, mapper, logger)
    {
        _clientService = service;
        _fiatAssetTransactionService = fiatAssetTransactionService;
    }

    /// <summary>
    /// Creates an entity from request - Client-specific implementation
    /// </summary>
    protected override async Task<Client> CreateEntityFromRequest(ClientRequest request)
    {
        return await _clientService.AddFromRequest(request);
    }

    /// <summary>
    /// Updates an entity from request - Client-specific implementation
    /// </summary>
    protected override async Task<Client> UpdateEntityFromRequest(Guid id, ClientRequest request)
    {
        return await _clientService.UpdateFromRequest(id, request);
    }

    /// <summary>
    /// Gets client-specific statistics including age information
    /// </summary>
    [HttpGet("{id}/client-statistics")]
    [ProducesResponseType(typeof(ClientStatistics), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClientStatistics(Guid id)
    {
        try
        {
            var statistics = await _clientService.GetClientStatistics(id);
            return Ok(statistics);
        }
        catch (Exception)
        {
            return HandleGenericException("retrieving client-specific statistics for");
        }
    }

    /// <summary>
    /// Send Brazilian Real transaction for client
    /// </summary>
    [Obsolete("Use POST /api/v1/transfer instead. Will be removed in v2.")]
    [HttpPost("{id}/send-brazilian-real")]
    [ProducesResponseType(typeof(FiatAssetTransaction), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendBrazilianReais(Guid id, [FromBody] FiatAssetTransactionRequest request)
    {
        try
        {
            var transaction = await _fiatAssetTransactionService.SendBrazilianReais(id, request);
            return Ok(transaction);
        }
        catch (Exception)
        {
            return HandleGenericException("processing Brazilian Real transaction for");
        }
    }

    /// <summary>
    /// Check if wallet identifier exists
    /// </summary>
    [HttpGet]
    [Route("wallet-identifier-has")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public IActionResult WalletIdentifierHas([FromQuery] string input)
    {
        try
        {
            // This endpoint needs to be implemented based on your business logic
            // For now, I'll add a placeholder that you can implement
            throw new NotImplementedException("WalletIdentifierHas endpoint needs to be implemented");
        }
        catch (Exception)
        {
            return HandleGenericException("checking wallet identifier for");
        }
    }

    /// <summary>
    /// Get or manage initial balance for client
    /// </summary>
    [HttpGet]
    [Route("initial-balance/{clientId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public IActionResult GetInitialBalance(Guid clientId)
    {
        try
        {
            // This endpoint needs to be implemented based on your business logic
            // For now, I'll add a placeholder that you can implement
            throw new NotImplementedException("GetInitialBalance endpoint needs to be implemented");
        }
        catch (Exception)
        {
            return HandleGenericException("retrieving initial balance for");
        }
    }
}