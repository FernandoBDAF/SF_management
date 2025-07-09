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
public class BankController : BaseApiController<Bank, BankRequest, BankResponse>
{
    private readonly BankService _bankService;
    private readonly IMapper _mapper;

    public BankController(BankService service, IMapper mapper) : base(service, mapper)
    {
        _bankService = service;
        _mapper = mapper;
    }

    [HttpPost]
    [Route("")]
    public override async Task<BankResponse> Post(BankRequest request)
    {
        var bank = await _bankService.AddFromRequest(request);
        return _mapper.Map<BankResponse>(bank);
    }

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
        return await _bankService.GetTransactionsStatementForAssetHolder(id);
    }
}
