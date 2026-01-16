using SFManagement.Application.Services.Transactions;
using SFManagement.Application.DTOs.Transactions;
using SFManagement.Api.Controllers.Base;
using SFManagement.Application.DTOs.AssetHolders;
using SFManagement.Application.Services.AssetHolders;
using SFManagement.Application.Services.Assets;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Api.Controllers.Base;
using SFManagement.Domain.Entities.AssetHolders;
using SFManagement.Domain.Entities.Transactions;
using SFManagement.Application.Services;
using SFManagement.Application.DTOs;

namespace SFManagement.Api.Controllers.v1.AssetHolders;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class MemberController : BaseAssetHolderController<Member, MemberRequest, MemberResponse>
{
    private readonly MemberService _memberService;
    private readonly FiatAssetTransactionService _fiatAssetTransactionService;
    private readonly WalletIdentifierService _walletIdentifierService;


    public MemberController(
        MemberService service, 
        FiatAssetTransactionService fiatAssetTransactionService, 
        IMapper mapper,
        ILogger<BaseAssetHolderController<Member, MemberRequest, MemberResponse>> logger,
        WalletIdentifierService walletIdentifierService)
        : base(service, walletIdentifierService, mapper, logger)
    {
        _memberService = service;
        _fiatAssetTransactionService = fiatAssetTransactionService;
    }

    /// <summary>
    /// Creates an entity from request - Member-specific implementation
    /// </summary>
    protected override async Task<Member> CreateEntityFromRequest(MemberRequest request)
    {
        return await _memberService.AddFromRequest(request);
    }

    /// <summary>
    /// Updates an entity from request - Member-specific implementation
    /// </summary>
    protected override async Task<Member> UpdateEntityFromRequest(Guid id, MemberRequest request)
    {
        return await _memberService.UpdateFromRequest(id, request);
    }

    /// <summary>
    /// Gets member-specific statistics including share information
    /// </summary>
    [HttpGet("{id}/member-statistics")]
    [ProducesResponseType(typeof(MemberStatistics), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMemberStatistics(Guid id)
    {
        try
        {
            var statistics = await _memberService.GetMemberStatistics(id);
            return Ok(statistics);
        }
        catch (Exception)
        {
            return HandleGenericException("retrieving member-specific statistics for");
        }
    }
    
    /// <summary>
    /// Send Brazilian Real transaction for member
    /// </summary>
    [HttpPost("{id}/send-brazilian-real")]
    [ProducesResponseType(typeof(FiatAssetTransaction), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendBrazilianReais(Guid id, [FromBody] FiatAssetTransactionRequest request)
    {
        try
        {
            var transaction = await _fiatAssetTransactionService.SendBrazilianReais(id, request);
            return Ok(transaction);
        }
        catch (Exception)
        {
            return HandleGenericException("processing Brazilian Real transaction for");
        }
    }
}