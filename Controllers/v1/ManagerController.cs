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
public class ManagerController : BaseApiController<PokerManager, PokerManagerRequest, PokerManagerResponse>
{
    private readonly PokerManagerService _pokerManagerService;

    private readonly IMapper _mapper;

    private readonly TransactionService _transactionService;
    private readonly WalletService _walletService;

    public ManagerController(BaseService<PokerManager> service, IMapper mapper, WalletService walletService,
        PokerManagerService pokerManagerService, TransactionService transactionService) : base(service, mapper)
    {
        _mapper = mapper;
        _walletService = walletService;
        _pokerManagerService = pokerManagerService;
        _transactionService = transactionService;
    }

    [HttpGet]
    [Route("wallets/{managerId}")]
    public async Task<List<WalletResponse>> GetWalletsByManagerId(Guid managerId)
    {
        return _mapper.Map<List<WalletResponse>>(await _walletService.GetWalletsByManagerId(managerId));
    }

    [HttpGet]
    [Route("balance/{managerId}")]
    public async Task<BalanceResponse> Balance(Guid managerId)
    {
        return await _pokerManagerService.GetBalance(managerId, null);
    }

    [HttpPost]
    [Route("balance/{managerId}")]
    public async Task<BalanceResponse> Balance(Guid managerId, BalanceRequest request)
    {
        return await _pokerManagerService.GetBalance(managerId, request.Date);
    }

    [HttpGet]
    [Route("profit/{managerId}")]
    public async Task<ProfitResponse> Profit(Guid managerId, DateTime? start, DateTime? end)
    {
        return await _pokerManagerService.GetProfit(managerId, start, end);
    }

    [HttpGet]
    [Route("transactions/{managerId}/{startDate?}/{endDate?}/{quantity?}/{page?}")]
    public async Task<TableResponse<TransactionResponse>> Transactions(Guid managerId, DateTime? startDate = null,
        DateTime? endDate = null, int quantity = 100, int page = 0)
    {
        return await _transactionService.GetManagerTransactions(managerId, startDate, endDate, quantity, page);
    }

    [HttpGet]
    [Route("transactions/{managerId}/{quantity?}/{page?}")]
    public async Task<TableResponse<TransactionResponse>> Transactions(Guid managerId, int quantity = 100, int page = 0)
    {
        return await _transactionService.GetManagerTransactions(managerId, null, null, quantity, page);
    }
}