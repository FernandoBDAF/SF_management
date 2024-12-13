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
    public class ManagerController : BaseApiController<Manager, ManagerRequest, ManagerResponse>
    {
        private readonly WalletService _walletService;

        private readonly ManagerService _managerService;

        private readonly IMapper _mapper;

        private readonly TransactionService _transactionService;

        public ManagerController(BaseService<Manager> service, IMapper mapper, WalletService walletService, ManagerService managerService, TransactionService transactionService) : base(service, mapper)
        {
            _mapper = mapper;
            _walletService = walletService;
            _managerService = managerService;
            _transactionService = transactionService;
        }

        [HttpGet("/wallets/{managerId}")]
        public async Task<List<WalletResponse>> GetWalletsByManagerId(Guid managerId) => _mapper.Map<List<WalletResponse>>(await _walletService.GetWalletsByManagerId(managerId));

        [HttpGet]
        [Route("balance/{managerId}")]
        public async Task<BalanceResponse> Balance(Guid managerId) => await _managerService.GetBalance(managerId);

        [HttpGet]
        [Route("transactions/{managerId}/{startDate?}/{endDate?}/{quantity?}/{page?}")]
        public async Task<TableResponse<TransactionResponse>> Transactions(Guid managerId, DateTime? startDate = null, DateTime? endDate = null, int quantity = 100, int page = 0) => await _transactionService.GetManagerTransactions(managerId, startDate, endDate, quantity, page);

        [HttpGet]
        [Route("transactions/{managerId}/{quantity?}/{page?}")]
        public async Task<TableResponse<TransactionResponse>> Transactions(Guid managerId, int quantity = 100, int page = 0) => await _transactionService.GetManagerTransactions(managerId, null, null, quantity, page);
    }
}
