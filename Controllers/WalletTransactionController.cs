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
        [Route("approve/{walletTransactionId}")]
        public async Task<WalletTransactionResponse> ApproveTransaction(Guid walletTransactionId, [FromBody] WalletTransactionApproveRequest model) => await _walletTransactionService.ApproveTransaction(walletTransactionId, model);
    }
}
