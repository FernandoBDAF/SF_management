using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Models.AssetInfrastructure;
using SFManagement.Services;
using SFManagement.ViewModels;

namespace SFManagement.Controllers.v1;

[ApiController]
[Route("api/v{verion:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class WalletIdentifierController(
    BaseService<WalletIdentifier> service,
    IMapper mapper,
    WalletIdentifierService walletIdentifierService)
    : BaseApiController<WalletIdentifier, WalletIdentifierRequest, WalletIdentifierResponse>(service, mapper)
{
    private readonly IMapper _mapper = mapper;

    // [HttpGet]
    // [Route("nickname-client/{clientId}")]
    // public async Task<List<WalletIdentifierResponse>> GetNicknames(Guid clientId)
    // {
    //     return _mapper.Map<List<WalletIdentifierResponse>>(await walletIdentifierService.GetByClientId(clientId));
    // }
}