using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Models;
using SFManagement.Models.Entities;
using SFManagement.Services;
using SFManagement.ViewModels;

namespace SFManagement.Controllers.v1;

[ApiController]
[Route("api/v{verion:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class NicknameController : BaseApiController<WalletIdentifier, NicknameRequest, NicknameResponse>
{
    private readonly IMapper _mapper;
    private readonly WalletIdentifierService _walletIdentifierService;

    public NicknameController(BaseService<WalletIdentifier> service, IMapper mapper, WalletIdentifierService walletIdentifierService) : base(
        service, mapper)
    {
        _walletIdentifierService = walletIdentifierService;
        _mapper = mapper;
    }

    [HttpGet]
    [Route("nickname-client/{clientId}")]
    public async Task<List<NicknameResponse>> GetNicknames(Guid clientId)
    {
        return _mapper.Map<List<NicknameResponse>>(await _walletIdentifierService.GetByClientId(clientId));
    }
}