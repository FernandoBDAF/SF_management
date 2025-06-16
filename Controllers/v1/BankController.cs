using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Models;
using SFManagement.Models.Entities;
using SFManagement.Services;
using SFManagement.ViewModels;

namespace SFManagement.Controllers.v1;

[ApiController]
[Route("api/v{verion:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class BankController : BaseApiController<Bank, BankRequest, BankResponse>
{
    private readonly BankService _bankService;
    private readonly TransactionService _transactionService;

    public BankController(BaseService<Bank> service, IMapper mapper, BankService bankService,
        TransactionService transactionService) : base(service, mapper)
    {
        _bankService = bankService;
        _transactionService = transactionService;
    }

    // [HttpGet]
    // [Route("balance/{bankId}")]
    // // [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, NoStore = false)]
    // public async Task<BalanceResponse> Balance(Guid bankId)
    // {
    //     return await _bankService.GetBalance(bankId, null);
    // }
    //
    // [HttpPost]
    // [Route("balance/{bankId}")]
    // public async Task<BalanceResponse> Balance(Guid bankId, BalanceRequest request)
    // {
    //     return await _bankService.GetBalance(bankId, request.Date);
    // }

    [HttpGet]
    [Route("transactions/{bankId}/{startDate?}/{endDate?}/{quantity?}/{page?}")]
    public async Task<TableResponse<TransactionResponse>> Transactions(Guid bankId, DateTime? startDate = null,
        DateTime? endDate = null, int? quantity = 100, int? page = 0)
    {
        return await _transactionService.GetBankTransactions(bankId, startDate, endDate, quantity.Value, page.Value);
    }

    [HttpGet]
    [Route("transactions/{bankId}/{quantity?}/{page?}")]
    public async Task<TableResponse<TransactionResponse>> Transactions(Guid bankId, int? quantity = 100, int? page = 0)
    {
        return await _transactionService.GetBankTransactions(bankId, null, null, quantity.Value, page.Value);
    }
}