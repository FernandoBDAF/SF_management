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
    public class ClosingWalletController : BaseApiController<ClosingWallet, ClosingWalletRequest, ClosingWalletResponse>
    {
        public ClosingWalletController(BaseService<ClosingWallet> service, IMapper mapper) : base(service, mapper)
        {
        }
    }
}
