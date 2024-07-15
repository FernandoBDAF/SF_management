using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Models;
using SFManagement.Services;
using SFManagement.ViewModels;

namespace SFManagement.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BankTransactionController : BaseApiController<BankTransaction, BankTransactionRequest, BankTransactionResponse>
    {
        public BankTransactionController(BaseService<BankTransaction> service, IMapper mapper) : base(service, mapper)
        {
        }
    }
}
