using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Models.Entities;
using SFManagement.Models.Transactions;
using SFManagement.Services;
using SFManagement.ViewModels;

namespace SFManagement.Controllers.v1;

[ApiController]
[Route("api/v{verion:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class MemberController(MemberService service, FiatAssetTransactionService fiatAssetTransactionService, IMapper mapper)
    : BaseApiController<Member, MemberRequest, MemberResponse>(service, mapper)
{
    private readonly IMapper _mapper = mapper;
    private readonly MemberService _memberService = service;
    private readonly FiatAssetTransactionService _fiatAssetTransactionService = fiatAssetTransactionService;
    
    
    [HttpPost]
    [Route("{memberId}/send-brazilian-real")]
    public async Task<FiatAssetTransaction> SendBrazilianReais(Guid memberId, FiatAssetTransactionRequest request)
    {
        var member = await _memberService.Get(memberId);
        
        return await _fiatAssetTransactionService.SendBrazilianReais(member, request);
    }
}