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
    public class InternalTransactionController : BaseApiController<InternalTransaction, InternalTransactionRequest, InternalTransactionResponse>
    {
        public InternalTransactionController(BaseService<InternalTransaction> service, IMapper mapper) : base(service, mapper)
        {
        }
    }
}
