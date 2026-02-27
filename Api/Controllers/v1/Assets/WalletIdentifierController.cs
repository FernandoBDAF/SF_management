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
using SFManagement.Infrastructure.Authorization;

namespace SFManagement.Api.Controllers.v1.Assets;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[RequirePermission(Auth0Permissions.ReadWallets)]
public class WalletIdentifierController(
    BaseService<WalletIdentifier> service,
    IMapper mapper,
    WalletIdentifierService walletIdentifierService)
    : BaseApiController<WalletIdentifier, WalletIdentifierRequest, WalletIdentifierResponse>(service, mapper)
{
    private readonly IMapper _mapper = mapper;

    [HttpPost("internal-wallet")]
    [RequirePermission(Auth0Permissions.CreateWallets)]
    public async Task<IActionResult> AddInternalWallet([FromBody] WalletIdentifierRequest request)
    {
        var walletIdentifier = _mapper.Map<WalletIdentifier>(request);
        var result = await walletIdentifierService.AddWithAssetGroup(walletIdentifier, AssetGroup.Internal);
        return Ok(result);
    }

    [HttpPost("settlement-wallet")]
    [RequirePermission(Auth0Permissions.CreateWallets)]
    public async Task<IActionResult> AddSettlementWallet([FromBody] WalletIdentifierRequest request)
    {
        var walletIdentifier = _mapper.Map<WalletIdentifier>(request);
        var result = await walletIdentifierService.AddWithAssetGroup(walletIdentifier, AssetGroup.Settlements);
        return Ok(result);
    }
    [RequirePermission(Auth0Permissions.CreateWallets)]
    public override Task<IActionResult> Post(WalletIdentifierRequest model)
    {
        return base.Post(model);
    }

    [RequireRole(Auth0Roles.Admin)]
    public override Task<IActionResult> Put(Guid id, WalletIdentifierRequest model)
    {
        return base.Put(id, model);
    }

    [RequireRole(Auth0Roles.Admin)]
    public override Task<IActionResult> Delete(Guid id)
    {
        return base.Delete(id);
    }
}