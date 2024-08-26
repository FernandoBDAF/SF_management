using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Enums;
using SFManagement.Models;
using SFManagement.Services;
using SFManagement.ViewModels;

namespace SFManagement.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class WalletTransactionController : BaseApiController<WalletTransaction, WalletTransactionRequest, WalletTransactionResponse>
    {
        private readonly WalletTransactionService _walletTransactionService;

        public WalletTransactionController(BaseService<WalletTransaction> service, IMapper mapper, WalletTransactionService walletTransactionService) : base(service, mapper)
        {
            _walletTransactionService = walletTransactionService;
        }

        [HttpPost]
        [Route("import-buy-transactions")]
        public async Task<List<WalletTransactionResponse>> ImportBuyTransactions(ImportBuySellTransactionsRequest request) => await _walletTransactionService.ImportBuySellTransactions(request, WalletTransactionType.Income);

        [HttpPost]
        [Route("import-sell-transactions")]
        public async Task<List<WalletTransactionResponse>> ImportSellTransactions(ImportBuySellTransactionsRequest request) => await _walletTransactionService.ImportBuySellTransactions(request, WalletTransactionType.Expense);

        [HttpPost]
        [Route("import-transfer-transactions")]
        public async Task<List<WalletTransactionResponse>> ImportTransferTransactions(ImportTransferTransactionRequest request) => await _walletTransactionService.ImportTransferTransactions(request);

        [HttpPost]
        [Route("approve/{walletTransactionId}")]
        public async Task<WalletTransactionResponse> ApproveTransaction(int walletTransactionId) => await _walletTransactionService.ApproveTransaction(walletTransactionId);
    }
}
