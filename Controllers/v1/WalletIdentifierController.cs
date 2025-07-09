using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Enums;
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
    
}