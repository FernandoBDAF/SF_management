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
        private readonly IMapper _mapper;

        public WalletTransactionController(BaseService<WalletTransaction> service, IMapper mapper, WalletTransactionService walletTransactionService) : base(service, mapper)
        {
            _walletTransactionService = walletTransactionService;
            _mapper = mapper;
        }

        [HttpPost]
        [Route("approve/{walletTransactionId}")]
        public async Task<WalletTransactionResponse> ApproveTransaction(Guid walletTransactionId, [FromBody] WalletTransactionApproveRequest model) => await _walletTransactionService.ApproveTransaction(walletTransactionId, model);

        [HttpPut]
        [Route("unapprove/{walletTransactionId}")]
        public async Task<WalletTransactionResponse> Unapprove(Guid walletTransactionId) => _mapper.Map<WalletTransactionResponse>(await _walletTransactionService.UnApproveTransaction(walletTransactionId));

        [HttpPut]
        [Route("link/{fromWalletTransactionId}/{toWalletTransactionId}")]
        public async Task<(WalletTransactionResponse from, WalletTransactionResponse to)> Link(Guid fromWalletTransactionId, Guid toWalletTransactionId) => _mapper.Map<(WalletTransactionResponse from, WalletTransactionResponse to)>(await _walletTransactionService.Link(fromWalletTransactionId, toWalletTransactionId));
    }
}
