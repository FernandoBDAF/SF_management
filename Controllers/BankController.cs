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
    public class BankController : BaseApiController<Bank, BankRequest, BankResponse>
    {
        private readonly BankService _bankService;
        private readonly TransactionService _transactionService;

        public BankController(BaseService<Bank> service, IMapper mapper, BankService bankService, TransactionService transactionService) : base(service, mapper)
        {
            _bankService = bankService;
            _transactionService = transactionService;
        }

        [HttpGet]
        [Route("balance/{bankId}")]
        public async Task<BalanceResponse> Balance(Guid bankId) => await _bankService.GetBalance(bankId);

        [HttpGet]
        [Route("transactions/{bankId}/{startDate?}/{endDate?}/{quantity?}/{page?}")]
        public async Task<TableResponse<TransactionResponse>> Transactions(Guid bankId, DateTime? startDate = null, DateTime? endDate = null, int? quantity = 100, int? page = 0) => await _transactionService.GetBankTransactions(bankId, startDate, endDate, quantity.Value, page.Value);
    }
}
