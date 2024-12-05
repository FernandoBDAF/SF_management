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

        public ManagerController(BaseService<Manager> service, IMapper mapper, WalletService walletService, ManagerService managerService) : base(service, mapper)
        {
            _mapper = mapper;
            _walletService = walletService;
            _managerService = managerService;
        }

        [HttpGet("/wallets/{managerId}")]
        public async Task<List<WalletResponse>> GetWalletsByManagerId(Guid managerId) => _mapper.Map<List<WalletResponse>>(await _walletService.GetWalletsByManagerId(managerId));


        [HttpGet]
        [Route("balance/{managerId}")]
        public async Task<BalanceResponse> Balance(Guid managerId) => await _managerService.GetBalance(managerId);
    }
}
