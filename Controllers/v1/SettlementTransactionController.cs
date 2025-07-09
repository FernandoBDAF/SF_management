using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Models.Transactions;
using SFManagement.Services;
using SFManagement.ViewModels;

namespace SFManagement.Controllers.v1;

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
            
            response.ClosingGroups.Add(new SettlementClosingGroup
            {
                Date = closing.Key,
                Transactions = transactions
            });
        }
        
        return response;
    }

    // [HttpGet]
    // [Route("settlement-transactions")]
    // public async Task<TableResponse<FiatAssetTransactionResponse>> BankTransactions([FromQuery] int? quantity, [FromQuery] int? page)
    // {
    //     var bankAssetWalletIds = await _bankService.GetAssetHolderAssetWalletIds();
    //     
    //     var response = new TableResponse<FiatAssetTransactionResponse>
    //     {
    //         Data = [],
    //         Total = 0
    //     };
    //
    //     if (bankAssetWalletIds.Length == 0)
    //     {
    //         return response;
    //     }
    //     
    //     var transactions = await _fiatAssetTransactionService
    //         .GetAssetHolderTransactions(bankAssetWalletIds, null, null, quantity ?? 100, page ?? 0);
    //     
    //     response.Total = transactions.Length;
    //     
    //     response.Data = _mapper.Map<List<FiatAssetTransactionResponse>>(transactions);
    //     
    //     return response;
    // }
}
