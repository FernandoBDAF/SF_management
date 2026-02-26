using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Api.Controllers.Base;
using SFManagement.Application.DTOs.Common;
using SFManagement.Application.DTOs.ImportedTransactions;
using SFManagement.Application.DTOs.Transactions;
using SFManagement.Application.Services.AssetHolders;
using SFManagement.Application.Services.Base;
using SFManagement.Application.Services.Transactions;
using SFManagement.Domain.Entities.Transactions;

namespace SFManagement.Api.Controllers.v1.Transactions;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class
    DigitalAssetTransactionController : BaseApiController<DigitalAssetTransaction, DigitalAssetTransactionRequest,
    DigitalAssetTransactionResponse>
{
    private readonly IMapper _mapper;
    private readonly DigitalAssetTransactionService _digitalAssetTransactionService;
    private readonly PokerManagerService _pokerManagerService;

    public DigitalAssetTransactionController(DigitalAssetTransactionService service,
        PokerManagerService pokerManagerService, IMapper mapper) 
        : base(service, mapper)
    {
        _digitalAssetTransactionService = service;
        _mapper = mapper;
        _pokerManagerService = pokerManagerService;
    }
    
    [HttpGet]
    [Route("poker-manager-transactions")]
    public async Task<TableResponse<DigitalAssetTransactionResponse>> Transactions([FromQuery] int? quantity, [FromQuery] int? page)
    {
        var pokerManagerAssetPoolIds = await _pokerManagerService.GetAssetHolderAssetPoolIds();
        
        var response = new TableResponse<DigitalAssetTransactionResponse>
        {
            Data = [],
            Total = 0
        };

        if (pokerManagerAssetPoolIds.Length == 0)
        {
            return response;
        }
        
        var transactions = await _digitalAssetTransactionService
            .GetAssetHolderTransactions(pokerManagerAssetPoolIds, null, null, quantity ?? 1000, page ?? 0);
        
        response.Total = transactions.Total;
        
        response.Data = _mapper.Map<List<DigitalAssetTransactionResponse>>(transactions.Data);
        
        return response;
    }

    // [HttpPost]
    // [Route("approve/{walletTransactionId}")]
    // public async Task<DigitalAssetTransactionResponse> ApproveTransaction(Guid walletTransactionId,
    //     [FromBody] WalletTransactionApproveRequest model)
    // {
    //     return await _digitalAssetTransactionService.ApproveTransaction(walletTransactionId, model);
    // }
    //
    // [HttpPut]
    // [Route("unapprove/{walletTransactionId}")]
    // public async Task<DigitalAssetTransactionResponse> Unapprove(Guid walletTransactionId)
    // {
    //     return _mapper.Map<DigitalAssetTransactionResponse>(
    //         await _digitalAssetTransactionService.UnApproveTransaction(walletTransactionId));
    // }
    //
    // [HttpPut]
    // [Route("link/{fromWalletTransactionId}/{toWalletTransactionId}")]
    // public async Task<(DigitalAssetTransactionResponse from, DigitalAssetTransactionResponse to)> Link(Guid fromWalletTransactionId,
    //     Guid toWalletTransactionId)
    // {
    //     return _mapper.Map<(DigitalAssetTransactionResponse from, DigitalAssetTransactionResponse to)>(
    //         await _digitalAssetTransactionService.Link(fromWalletTransactionId, toWalletTransactionId));
    // }

    [NonAction]
    public override Task<IActionResult> Put(Guid id, DigitalAssetTransactionRequest model)
    {
        return Task.FromResult<IActionResult>(StatusCode(StatusCodes.Status405MethodNotAllowed));
    }

    [HttpPut]
    [Route("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDigitalAssetTransactionRequest model)
    {
        try
        {
            var result = await _digitalAssetTransactionService.PartialUpdateAsync(id, model);
            var response = _mapper.Map<DigitalAssetTransactionResponse>(result);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}