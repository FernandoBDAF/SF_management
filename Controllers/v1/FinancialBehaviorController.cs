using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Models;
using SFManagement.Services;
using SFManagement.ViewModels;

namespace SFManagement.Controllers.v1;

[ApiController]
[Route("api/v{verion:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class FinancialBehaviorController : BaseApiController<FinancialBehavior, FinancialBehaviorRequest, 
    FinancialBehaviorResponse>
{
    private readonly IMapper _mapper;
    private readonly FinancialBehaviorService _financialBehaviorService;

    public FinancialBehaviorController(FinancialBehaviorService financialBehavior, 
        BaseService<FinancialBehavior> service, IMapper mapper) : base(service, mapper)
    {
        _financialBehaviorService = financialBehavior;
        _mapper = mapper;
    }

    public override async Task<List<FinancialBehaviorResponse>> Get()
    {
        return _mapper.Map<List<FinancialBehaviorResponse>>(await _financialBehaviorService.List());
    }

    [HttpGet]
    [Route("balance/{tagId}")]
    public async Task<BalanceResponse> Balance(Guid tagId)
    {
        return await _financialBehaviorService.GetBalance(tagId);
    }

    // [HttpGet]
    // [Route("transactions/{tagId}/{startDate?}/{endDate?}/{quantity?}/{page?}")]
    // public async Task<TableResponse<TransactionResponse>> InternalTransactions(Guid tagId, DateTime? startDate = null,
    //     DateTime? endDate = null, int? quantity = 100, int? page = 0)
    // {
    //     return await _transactionService.GetTagTransactions(tagId, startDate, endDate, quantity.Value, page.Value);
    // }
    //
    // [HttpGet]
    // [Route("transactions/{tagId}/{quantity?}/{page?}")]
    // public async Task<TableResponse<TransactionResponse>> InternalTransactions(Guid tagId, int? quantity = 100,
    //     int? page = 0)
    // {
    //     return await _transactionService.GetTagTransactions(tagId, null, null, quantity.Value, page.Value);
    // }
}