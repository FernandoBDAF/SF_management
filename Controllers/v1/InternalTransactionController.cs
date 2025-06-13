using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Models;
using SFManagement.Models.Transactions;
using SFManagement.Services;
using SFManagement.ViewModels;

namespace SFManagement.Controllers.v1;

[ApiController]
[Route("api/v{verion:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class InternalTransactionController : BaseApiController<InternalTransaction, InternalTransactionRequest,
    InternalTransactionResponse>
{
    private readonly InternalTransactionService _internalTransactionService;
    private readonly IMapper _mapper;

    public InternalTransactionController(BaseService<InternalTransaction> service, IMapper mapper,
        InternalTransactionService internalTransactionService) : base(service, mapper)
    {
        _internalTransactionService = internalTransactionService;
        _mapper = mapper;
    }

    [HttpPost]
    [Route("transfer/{toId}/{fromId}")]
    public async Task<List<InternalTransactionResponse>> Transfer(Guid toId, Guid fromId,
        [FromBody] InternalTransactionTransferRequest model)
    {
        return _mapper.Map<List<InternalTransactionResponse>>(
            await _internalTransactionService.Transfer(toId, fromId, model));
    }


    [HttpPut]
    [Route("approve/{internalTransactionId}")]
    public async Task<InternalTransactionResponse> Approve(Guid internalTransactionId,
        [FromBody] InternalTransactionApproveRequest model)
    {
        return _mapper.Map<InternalTransactionResponse>(
            await _internalTransactionService.Approve(internalTransactionId, model));
    }

    [HttpPut]
    [Route("unapprove/{internalTransactionId}")]
    public async Task<InternalTransactionResponse> Unapprove(Guid internalTransactionId)
    {
        return _mapper.Map<InternalTransactionResponse>(
            await _internalTransactionService.Unapprove(internalTransactionId));
    }
}