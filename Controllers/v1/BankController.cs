using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Models.Entities;
using SFManagement.Services;
using SFManagement.ViewModels;

namespace SFManagement.Controllers.v1;

[ApiController]
[Route("api/v{verion:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class BankController(BaseService<Bank> service, IMapper mapper, BankService bankService) : BaseApiController<Bank, BankRequest, BankResponse>(service, mapper)
{
    private readonly BankService _bankService = bankService;

    // [HttpGet]
    // [Route("balance/{bankId}")]
    // // [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, NoStore = false)]
    // public async Task<BalanceResponse> Balance(Guid bankId)
    // {
    //     return await _bankService.GetBalance(bankId, null);
    // }

    // [HttpPost]
    // [Route("balance/{bankId}")]
    // public async Task<BalanceResponse> Balance(Guid bankId, BalanceRequest request)
    // {
    //     return await _bankService.GetBalance(bankId, request.Date);
    // }
}