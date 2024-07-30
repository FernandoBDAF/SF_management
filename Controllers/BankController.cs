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
        private readonly BankService _bankService;

        public BankController(BaseService<Bank> service, IMapper mapper, BankService bankService) : base(service, mapper)
        {
            _bankService = bankService;
        }

        [Route("balance/{id}")]
        public async Task<BalanceResponse> Balance(Guid bankId) => await _bankService.GetBalance(bankId);
    }
}
