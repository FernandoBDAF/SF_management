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
    public class WalletTransactionController : BaseApiController<WalletTransaction, WalletTransactionRequest, WalletTransactionResponse>
    {
        public WalletTransactionController(BaseService<WalletTransaction> service, IMapper mapper) : base(service, mapper)
        {
        }
    }
}
