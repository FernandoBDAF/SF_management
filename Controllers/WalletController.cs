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
        public WalletService _walletService;

        public WalletController(WalletService walletService, BaseService<Wallet> service, IMapper mapper) : base(service, mapper)
        {
            _walletService = walletService;
        }

        [HttpGet]
        [Route("balance/{walletId}")]
        public async Task<BalanceResponse> Balance(Guid walletId) => await _walletService.GetBalance(walletId);
    }
}
