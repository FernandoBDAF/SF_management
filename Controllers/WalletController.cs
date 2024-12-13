using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Models;
using SFManagement.Services;
using SFManagement.ViewModels;

namespace SFManagement.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class WalletController : BaseApiController<Wallet, WalletRequest, WalletResponse>
    {
        private WalletService _walletService;

        private readonly TransactionService _transactionService;

        public WalletController(WalletService walletService, BaseService<Wallet> service, IMapper mapper, TransactionService transactionService) : base(service, mapper)
        {
            _walletService = walletService;
            _transactionService = transactionService;
        }

        [HttpGet]
        [Route("balance/{walletId}")]
        public async Task<BalanceResponse> Balance(Guid walletId) => await _walletService.GetBalance(walletId);

        [HttpGet]
        [Route("transactions/{walletId}/{startDate?}/{endDate?}/{quantity?}/{page?}")]
        public async Task<TableResponse<TransactionResponse>> Transactions(Guid walletId, DateTime? startDate = null, DateTime? endDate = null, int? quantity = 100, int? page = 0) => await _transactionService.GetWalletTransactions(walletId, startDate, endDate, quantity.Value, page.Value);

        [HttpGet]
        [Route("transactions/{walletId}/{quantity?}/{page?}")]
        public async Task<TableResponse<TransactionResponse>> Transactions(Guid walletId, int? quantity = 100, int? page = 0) => await _transactionService.GetWalletTransactions(walletId, null, null, quantity.Value, page.Value);
    }
}
