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
        public WalletController(BaseService<Wallet> service, IMapper mapper) : base(service, mapper)
        {
        }
    }
}
