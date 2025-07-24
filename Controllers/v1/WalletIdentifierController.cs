using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Enums;
using SFManagement.Enums.AssetInfrastructure;
using SFManagement.Models.AssetInfrastructure;
using SFManagement.Services;
using SFManagement.ViewModels;

namespace SFManagement.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class WalletIdentifierController(
    BaseService<WalletIdentifier> service,
    IMapper mapper,
    WalletIdentifierService walletIdentifierService)
    : BaseApiController<WalletIdentifier, WalletIdentifierRequest, WalletIdentifierResponse>(service, mapper)
{
    private readonly IMapper _mapper = mapper;

    [HttpPost("internal-wallet")]
    public async Task<IActionResult> AddInternalWallet([FromBody] WalletIdentifierRequest request)
    {
        var walletIdentifier = _mapper.Map<WalletIdentifier>(request);
        var result = await walletIdentifierService.AddWithAssetGroup(walletIdentifier, AssetGroup.Internal);
        return Ok(result);
    }

    [HttpPost("settlement-wallet")]
    public async Task<IActionResult> AddSettlementWallet([FromBody] WalletIdentifierRequest request)
    {
        var walletIdentifier = _mapper.Map<WalletIdentifier>(request);
        var result = await walletIdentifierService.AddWithAssetGroup(walletIdentifier, AssetGroup.Settlements);
        return Ok(result);
    }
    
}