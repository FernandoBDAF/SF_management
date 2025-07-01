using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Models.Entities;
using SFManagement.Services;
using SFManagement.ViewModels;
using SFManagement.Enums;

namespace SFManagement.Controllers.v1;

[ApiController]
[Route("api/v{verion:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class BankController(BaseService<Bank> service, IMapper mapper, BankService bankService) : BaseApiController<Bank, BankRequest, BankResponse>(service, mapper)
{
    private readonly BankService _bankService = bankService;

    [HttpGet]
    [Route("{id}/balance")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, NoStore = false)]
    public async Task<Dictionary<AssetType,decimal>> Balance(Guid id)
    {
        return await _bankService.GetBalancesByAssetType(id);
    }

    [HttpGet]
    [Route("{id}/transactions")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, NoStore = false)]
    public async Task<StatementAssetHolderWithTransactions> GetAssetHolderWithTransactions(Guid id)
    {
        return await _bankService.GetAssetHolderWithTransactionsAsStatement(id);
    }
}
