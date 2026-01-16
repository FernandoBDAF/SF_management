using SFManagement.Application.Services.AssetHolders;
using SFManagement.Application.DTOs.Common;
using SFManagement.Api.Controllers.Base;
using SFManagement.Application.DTOs.Transactions;
using SFManagement.Application.DTOs.ImportedTransactions;
using SFManagement.Application.Services.Transactions;
using SFManagement.Application.Services.Base;
﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Domain.Entities.Transactions;
using SFManagement.Application.Services;
using SFManagement.Application.DTOs;

namespace SFManagement.Api.Controllers.v1.Transactions;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
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
}