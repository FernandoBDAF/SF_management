using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Api.Controllers.Base;
using SFManagement.Application.DTOs.Common;
using SFManagement.Application.DTOs.ImportedTransactions;
using SFManagement.Application.DTOs.Transactions;
using SFManagement.Application.Services.Base;
using SFManagement.Application.Services.Transactions;
using SFManagement.Domain.Entities.Transactions;

namespace SFManagement.Api.Controllers.v1.Transactions;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class
    SettlementTransactionController : BaseApiController<SettlementTransaction, SettlementTransactionRequest,
    SettlementTransactionResponse>
{
    private readonly IMapper _mapper;
    private readonly SettlementTransactionService _service;

    public SettlementTransactionController(SettlementTransactionService service,
        IMapper mapper) : base(service, mapper) 
    {
        _mapper = mapper;
        _service = service;
    }
    
    [HttpGet]
    [Route("closings/{pokerManagerId}")]
    public async Task<SettlementClosingsGroupedResponse> GetClosingsGrouped(Guid pokerManagerId)
    {
        var closings = await _service.GetClosings(pokerManagerId);
        
        var response = new SettlementClosingsGroupedResponse();
        
        foreach (var closing in closings.OrderByDescending(c => c.Key))
        {
            var transactions = _mapper.Map<List<SettlementTransactionResponse>>(closing.Value);

            foreach (var transaction in transactions)
            {
                transaction.SignedAssetAmount = GetSignedAssetAmount(transaction, pokerManagerId);
            }
            
            response.ClosingGroups.Add(new SettlementClosingGroup
            {
                Date = closing.Key,
                Transactions = transactions
            });
        }
        
        return response;
    }

    private static decimal GetSignedAssetAmount(SettlementTransactionResponse transaction, Guid pokerManagerId)
    {
        var senderId = transaction.SenderWallet?.AssetHolder?.BaseAssetHolderId;
        var receiverId = transaction.ReceiverWallet?.AssetHolder?.BaseAssetHolderId;

        if (senderId == pokerManagerId)
        {
            return transaction.AssetAmount;
        }

        if (receiverId == pokerManagerId)
        {
            return -transaction.AssetAmount;
        }

        return transaction.AssetAmount;
    }

    // [HttpGet]
    // [Route("settlement-transactions")]
    // public async Task<TableResponse<FiatAssetTransactionResponse>> BankTransactions([FromQuery] int? quantity, [FromQuery] int? page)
    // {
    //     var bankAssetPoolIds = await _bankService.GetAssetHolderAssetPoolIds();
    //     
    //     var response = new TableResponse<FiatAssetTransactionResponse>
    //     {
    //         Data = [],
    //         Total = 0
    //     };
    //
    //     if (bankAssetPoolIds.Length == 0)
    //     {
    //         return response;
    //     }
    //     
    //     var transactions = await _fiatAssetTransactionService
    //         .GetAssetHolderTransactions(bankAssetPoolIds, null, null, quantity ?? 100, page ?? 0);
    //     
    //     response.Total = transactions.Length;
    //     
    //     response.Data = _mapper.Map<List<FiatAssetTransactionResponse>>(transactions);
    //     
    //     return response;
    // }
}
