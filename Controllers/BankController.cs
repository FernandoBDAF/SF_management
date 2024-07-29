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
    public class BankController : BaseApiController<Bank, BankRequest, BankResponse>
    {
        public BankController(BaseService<Bank> service, IMapper mapper) : base(service, mapper)
        {
        }
    }
}
