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
public class MemberController : BaseApiController<Member, MemberRequest, MemberResponse>
{
    private readonly IMapper _mapper;
    private readonly MemberService _memberService;
    private readonly FiatAssetTransactionService _fiatAssetTransactionService;

    public MemberController(MemberService service, FiatAssetTransactionService fiatAssetTransactionService, IMapper mapper)
        : base(service, mapper)
    {
        _mapper = mapper;
        _memberService = service;
        _fiatAssetTransactionService = fiatAssetTransactionService;
    }

    [HttpPost]
    [Route("")]
    public virtual async Task<MemberResponse> Post(MemberRequest request)
    {
        var member = await _memberService.AddFromRequest(request);
        return _mapper.Map<MemberResponse>(member);
    }
    
    [HttpPost]
    [Route("id/send-brazilian-real")]
    public async Task<FiatAssetTransaction> SendBrazilianReais(Guid id, FiatAssetTransactionRequest request)
    {
        return await _fiatAssetTransactionService.SendBrazilianReais(id, request);
    }
}