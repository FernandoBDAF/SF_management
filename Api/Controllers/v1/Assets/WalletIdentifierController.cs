using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Api.Controllers.Base;
using SFManagement.Application.DTOs.Assets;
using SFManagement.Application.DTOs.CompanyAssets;
using SFManagement.Application.Services.Assets;
using SFManagement.Application.Services.Base;
using SFManagement.Domain.Entities.Assets;
using SFManagement.Domain.Enums;
using SFManagement.Domain.Enums.Assets;

namespace SFManagement.Api.Controllers.v1.Assets;

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