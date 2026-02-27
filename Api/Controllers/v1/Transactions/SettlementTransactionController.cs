using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Api.Controllers.Base;
using SFManagement.Application.DTOs.Common;
using SFManagement.Application.DTOs.ImportedTransactions;
using SFManagement.Application.DTOs.Transactions;
using SFManagement.Application.Services.Base;
using SFManagement.Application.Services.Transactions;
using SFManagement.Domain.Entities.Transactions;
using SFManagement.Infrastructure.Authorization;

namespace SFManagement.Api.Controllers.v1.Transactions;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[RequirePermission(Auth0Permissions.ReadSettlements)]
public class
    SettlementTransactionController : BaseApiController<SettlementTransaction, SettlementTransactionRequest,
    SettlementTransactionResponse>
{
    private readonly IMapper _mapper;
    private readonly SettlementTransactionService _service;

    public SettlementTransactionController(SettlementTransactionService service,
        IMapper mapper) : base(service, mapper) 
    {
        _mapper = mapper;
        _service = service;
    }

    [HttpPost]
    [Route("")]
    [RequirePermission(Auth0Permissions.CreateSettlements)]
    [ProducesResponseType(StatusCodes.Status405MethodNotAllowed)]
    public override Task<IActionResult> Post(SettlementTransactionRequest model)
    {
        return Task.FromResult<IActionResult>(
            StatusCode(
                StatusCodes.Status405MethodNotAllowed,
                "Settlement transactions must be created via /api/v1/pokermanager/{id}/settlement-by-date."
            )
        );
    }
    
    [HttpGet]
    [Route("closings/{pokerManagerId}")]
    [RequirePermission(Auth0Permissions.ReadSettlements)]
    public async Task<SettlementClosingsGroupedResponse> GetClosingsGrouped(Guid pokerManagerId)
    {
        var closings = await _service.GetClosings(pokerManagerId);
        
        var response = new SettlementClosingsGroupedResponse();
        
        foreach (var closing in closings.OrderByDescending(c => c.Key))
        {
            var transactions = _mapper.Map<List<SettlementTransactionResponse>>(closing.Value);

            foreach (var transaction in transactions)
            {
                transaction.SignedAssetAmount = GetSignedAssetAmount(transaction, pokerManagerId);
            }
            
            response.ClosingGroups.Add(new SettlementClosingGroup
            {
                Date = closing.Key,
                Transactions = transactions
            });
        }
        
        return response;
    }

    private static decimal GetSignedAssetAmount(SettlementTransactionResponse transaction, Guid pokerManagerId)
    {
        var senderId = transaction.SenderWallet?.AssetHolder?.BaseAssetHolderId;
        var receiverId = transaction.ReceiverWallet?.AssetHolder?.BaseAssetHolderId;

        if (senderId == pokerManagerId)
        {
            return transaction.AssetAmount;
        }

        if (receiverId == pokerManagerId)
        {
            return -transaction.AssetAmount;
        }

        return transaction.AssetAmount;
    }
}
