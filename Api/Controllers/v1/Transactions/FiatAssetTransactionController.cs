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
using SFManagement.Infrastructure.Authorization;

namespace SFManagement.Api.Controllers.v1.Transactions;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[RequirePermission(Auth0Permissions.ReadTransactions)]
public class
    FiatAssetTransactionController : BaseApiController<FiatAssetTransaction, FiatAssetTransactionRequest, FiatAssetTransactionResponse>
{
    private readonly FiatAssetTransactionService _fiatAssetTransactionService;
    private readonly IMapper _mapper;
    private readonly BankService _bankService;

    public FiatAssetTransactionController(FiatAssetTransactionService service,
        BankService bankService ,IMapper mapper) : base(service, mapper)
    {
        _fiatAssetTransactionService = service;
        _mapper = mapper;
        _bankService = bankService;
    }
    
    [HttpGet]
    [Route("bank-transactions")]
    [RequirePermission(Auth0Permissions.ReadTransactions)]
    public async Task<TableResponse<FiatAssetTransactionResponse>> BankTransactions([FromQuery] int? quantity, [FromQuery] int? page)
    {
        var bankAssetPoolIds = await _bankService.GetAssetHolderAssetPoolIds();
        
        var response = new TableResponse<FiatAssetTransactionResponse>
        {
            Data = [],
            Total = 0
        };

        if (bankAssetPoolIds.Length == 0)
        {
            return response;
        }
        
        var transactions = await _fiatAssetTransactionService
            .GetAssetHolderTransactions(bankAssetPoolIds, null, null, quantity ?? 1000, page ?? 0);
        
        response.Total = transactions.Total;
        
        response.Data = _mapper.Map<List<FiatAssetTransactionResponse>>(transactions.Data);
        
        return response;
    }
    
    [HttpGet]
    [Route("direct-transactions")]
    [RequirePermission(Auth0Permissions.ReadTransactions)]
    public async Task<TableResponse<FiatAssetTransactionResponse>> DirectTransactions([FromQuery] int? quantity, [FromQuery] int? page)
    {
        var bankAssetPoolIds = await _bankService.GetAssetHolderAssetPoolIds();
        
        var response = new TableResponse<FiatAssetTransactionResponse>
        {
            Data = [],
            Total = 0
        };

        var transactions = await _fiatAssetTransactionService
            .GetNonAssetHolderTransactions(bankAssetPoolIds, null, null, quantity ?? 1000, page ?? 0);
        
        response.Total = transactions.Total;
        
        response.Data = _mapper.Map<List<FiatAssetTransactionResponse>>(transactions.Data);
        
        return response;
    }

    // [HttpPut]
    // [Route("approve/{bankTransactionId}")]
    // public async Task<FiatAssetTransactionResponse> Approve(Guid bankTransactionId,
    //     [FromBody] BankTransactionApproveRequest model)
    // {
    //     return _mapper.Map<FiatAssetTransactionResponse>(await _fiatAssetTransactionService.Approve(bankTransactionId, model));
    // }
    //
    // [HttpPut]
    // [Route("unapprove/{bankTransactionId}")]
    // public async Task<FiatAssetTransactionResponse> Unapprove(Guid bankTransactionId)
    // {
    //     return _mapper.Map<FiatAssetTransactionResponse>(await _fiatAssetTransactionService.Unapprove(bankTransactionId));
    // }
    //
    // [HttpPut]
    // [Route("link/{fromBankTransactionId}/{toBankTransactionId}")]
    // public async Task<(FiatAssetTransactionResponse from, FiatAssetTransactionResponse to)> Link(Guid fromBankTransactionId,
    //     Guid toBankTransactionId)
    // {
    //     return _mapper.Map<(FiatAssetTransactionResponse from, FiatAssetTransactionResponse to)>(
    //         await _fiatAssetTransactionService.Link(fromBankTransactionId, toBankTransactionId));
    // }
    //
    // [HttpGet]
    // [Route("list/{clientId}/{bankId}")]
    // public async Task<List<FiatAssetTransactionResponse>> ListByClienteId(Guid? clientId, Guid? bankId)
    // {
    //     return _mapper.Map<List<FiatAssetTransactionResponse>>(
    //         await _fiatAssetTransactionService.ListByClientIdAndBankId(clientId, bankId));
    // }

    [NonAction]
    public override Task<IActionResult> Put(Guid id, FiatAssetTransactionRequest model)
    {
        return Task.FromResult<IActionResult>(StatusCode(StatusCodes.Status405MethodNotAllowed));
    }

    [HttpPut]
    [Route("{id}")]
    [RequirePermission(Auth0Permissions.UpdateTransactions)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFiatAssetTransactionRequest model)
    {
        try
        {
            var result = await _fiatAssetTransactionService.PartialUpdateAsync(id, model);
            var response = _mapper.Map<FiatAssetTransactionResponse>(result);
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

    [RequirePermission(Auth0Permissions.CreateTransactions)]
    public override Task<IActionResult> Post(FiatAssetTransactionRequest model)
    {
        return base.Post(model);
    }

    [RequirePermission(Auth0Permissions.DeleteTransactions)]
    public override Task<IActionResult> Delete(Guid id)
    {
        return base.Delete(id);
    }
}